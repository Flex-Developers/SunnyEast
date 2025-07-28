using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace WebApi.Services.DbSnapshot;

public sealed class DbSnapshotService : IDbSnapshotService
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly IConfiguration _cfg;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DbSnapshotService> _log;   // <— NEW

    private readonly string _connString;
    private readonly string _backupDir;
    private readonly string _prefix;
    private readonly bool _runMigrations;
    private readonly int _keepLocal;

    public DbSnapshotService(IConfiguration cfg, ApplicationDbContext db, ILogger<DbSnapshotService> log) // <— NEW
    {
        _cfg = cfg;
        _db  = db;
        _log = log;

        _connString = cfg.GetConnectionString("mySql")
                      ?? throw new InvalidOperationException("Connection string 'mySql' not found.");

        _backupDir      = _cfg["Backups:Directory"] ?? "App_Data/Backups";
        _prefix         = _cfg["Backups:FilePrefix"] ?? "snapshot";
        _runMigrations  = bool.TryParse(_cfg["Backups:RunMigrationsAfterRestore"], out var b) && b;
        _keepLocal      = int.TryParse(_cfg["Backups:KeepLastLocalCopies"], out var k) ? Math.Max(0, k) : 10;

        Directory.CreateDirectory(_backupDir);
    }

    public async Task<string> CreateBackupGzipAsync(CancellationToken ct = default, bool skipGate = false)
    {
        if (!skipGate)
            await Gate.WaitAsync(ct);

        var sw = Stopwatch.StartNew();
        try
        {
            var stamp  = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var tmpSql = Path.Combine(Path.GetTempPath(), $"{_prefix}-{stamp}.sql");
            var gzPath = Path.Combine(Path.GetTempPath(), $"{_prefix}-{stamp}.sql.gz");

            _log.LogInformation("DB BACKUP started. tmpSql={Tmp}, gz={Gz}", tmpSql, gzPath);

            using (var conn = new MySqlConnection(_connString))
            using (var cmd  = new MySqlCommand())
            using (var mb   = new MySqlBackup(cmd))
            {
                cmd.Connection     = conn;
                cmd.CommandTimeout = 0; // безлимит
                await conn.OpenAsync(ct);

                mb.ExportInfo.AddCreateDatabase  = true;
                mb.ExportInfo.AddDropDatabase    = false;
                mb.ExportInfo.AddDropTable       = true;
                mb.ExportInfo.ExportFunctions    = true;
                mb.ExportInfo.ExportProcedures   = true;
                mb.ExportInfo.ExportTriggers     = true;
                mb.ExportInfo.ExportViews        = true;
                mb.ExportInfo.ExportEvents       = true;
                mb.ExportInfo.ResetAutoIncrement = false;
                mb.ExportInfo.RecordDumpTime     = true;

                mb.ExportToFile(tmpSql);
            }

            _log.LogInformation("DB BACKUP export done in {Elapsed} ms. Compressing...", sw.ElapsedMilliseconds);

            await using (var input = File.OpenRead(tmpSql))
            await using (var output = File.Create(gzPath))
            await using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
            {
                await input.CopyToAsync(gzip, 1024 * 1024, ct);
            }

            try { File.Delete(tmpSql); } catch { /* ignore */ }

            _log.LogInformation("DB BACKUP finished in {Elapsed} ms. gz={Gz}", sw.ElapsedMilliseconds, gzPath);
            return gzPath;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "DB BACKUP failed.");
            throw;
        }
        finally
        {
            if (!skipGate)
                Gate.Release();
        }
    }

    public async Task RestoreFromDumpAsync(Stream uploadedFile, string? fileName, CancellationToken ct = default)
    {
        await Gate.WaitAsync(ct);       // держим глобальную блокировку на весь restore
        var swTotal = Stopwatch.StartNew();
        try
        {
            var stamp    = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var safeName = MakeSafeFileName(fileName) ?? $"{_prefix}-{stamp}.sql";
            var tmpPath  = Path.Combine(Path.GetTempPath(), safeName);

            _log.LogInformation("DB RESTORE started. Incoming file={Name}", fileName);

            await using (var fs = File.Create(tmpPath))
                await uploadedFile.CopyToAsync(fs, ct);

            _log.LogInformation("Uploaded saved to {Tmp}. Size={Size} bytes", tmpPath, new FileInfo(tmpPath).Length);

            // ⬇⬇⬇ ВАЖНО: автобэкап БЕЗ повторного захвата Gate
            try
            {
                var backupGz  = await CreateBackupGzipAsync(ct, skipGate: true);
                var finalName = $"{_prefix}-AUTO-{stamp}.sql.gz";
                var dest      = Path.Combine(_backupDir, finalName);
                File.Move(backupGz, dest, overwrite: true);
                RotateBackups();
                _log.LogInformation("Auto-backup saved to {Dest}", dest);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Auto-backup failed. Continue restore anyway.");
            }

            var isGzip = tmpPath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
            string sqlPath;
            if (isGzip)
            {
                sqlPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(tmpPath));
                await using var input = File.OpenRead(tmpPath);
                await using var gz    = new GZipStream(input, CompressionMode.Decompress);
                await using var outFs = File.Create(sqlPath);
                await gz.CopyToAsync(outFs, ct);
                _log.LogInformation("Decompressed to {Sql}. Size={Size} bytes", sqlPath, new FileInfo(sqlPath).Length);
            }
            else
            {
                sqlPath = tmpPath;
            }

            _log.LogInformation("Importing SQL via MySqlBackup...");

            using (var conn = new MySqlConnection(_connString))
            using (var cmd  = new MySqlCommand())
            using (var mb   = new MySqlBackup(cmd))
            {
                cmd.Connection     = conn;
                cmd.CommandTimeout = 0; // безлимит
                await conn.OpenAsync(ct);

                var errLog = Path.Combine(_backupDir, $"import-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log");
                mb.ImportInfo.ErrorLogFile   = errLog;
                mb.ImportInfo.IgnoreSqlError = false;

                mb.ImportFromFile(sqlPath);
                _log.LogInformation("Import completed. SqlErrorLog={ErrLog}", errLog);
            }

            if (_runMigrations)
            {
                _log.LogInformation("Running EF migrations...");
                await _db.Database.MigrateAsync(ct);
                _log.LogInformation("EF migrations completed.");
            }

            try { if (isGzip && File.Exists(sqlPath)) File.Delete(sqlPath); } catch { }
            try { File.Delete(tmpPath); } catch { }

            _log.LogInformation("DB RESTORE finished in {Elapsed} ms.", swTotal.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "DB RESTORE failed.");
            throw;
        }
        finally
        {
            Gate.Release();
        }
    }

    private void RotateBackups()
    {
        if (_keepLocal <= 0) return;

        var files = Directory.GetFiles(_backupDir, $"{_prefix}-AUTO-*.sql.gz")
            .OrderByDescending(f => f)
            .ToList();

        if (files.Count <= _keepLocal) return;

        foreach (var f in files.Skip(_keepLocal))
        {
            try { File.Delete(f); } catch { /* ignore */ }
        }
    }

    private static string? MakeSafeFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = Path.GetFileName(name);
        name = Regex.Replace(name, @"[^a-zA-Z0-9_\-\.]+", "_");
        return name;
    }
}
