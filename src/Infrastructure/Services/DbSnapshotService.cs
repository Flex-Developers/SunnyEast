using MySqlConnector;

namespace Infrastructure.Services;

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Contract.Database.Responses;
using Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

public sealed class DbSnapshotService : IDbSnapshotService
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly IConfiguration _cfg;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DbSnapshotService> _log;

    private readonly string _connString;
    private readonly string _backupDir;
    private readonly string _prefix;
    private readonly bool _runMigrations;
    private readonly int _keepLocal;

    public DbSnapshotService(IConfiguration cfg, ApplicationDbContext db, ILogger<DbSnapshotService> log)
    {
        _cfg = cfg;
        _db = db;
        _log = log;

        _connString = cfg.GetConnectionString("mySql")
                      ?? throw new InvalidOperationException("Connection string 'mySql' not found.");

        _backupDir = _cfg["Backups:Directory"] ?? "App_Data/Backups";
        _prefix = _cfg["Backups:FilePrefix"] ?? "snapshot";
        _runMigrations = bool.TryParse(_cfg["Backups:RunMigrationsAfterRestore"], out var b) && b;
        _keepLocal = int.TryParse(_cfg["Backups:KeepLastLocalCopies"], out var k) ? Math.Max(0, k) : 10;

        Directory.CreateDirectory(_backupDir);
    }

    public async Task<DatabaseBackupResponse> CreateBackupAsync(CancellationToken ct)
    {
        await Gate.WaitAsync(ct);
        var sw = Stopwatch.StartNew();

        try
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var tmpSql = Path.Combine(Path.GetTempPath(), $"{_prefix}-{stamp}.sql");
            var tmpGz = Path.Combine(Path.GetTempPath(), $"{_prefix}-{stamp}.sql.gz");

            _log.LogInformation("DB BACKUP started. tmpSql={Tmp}, tmpGz={Gz}", tmpSql, tmpGz);

            await ExportToSqlAsync(tmpSql, ct);

            _log.LogInformation("DB BACKUP export done in {Elapsed} ms. Compressing...", sw.ElapsedMilliseconds);

            await CompressGzipAsync(tmpSql, tmpGz, ct);

            TryDelete(tmpSql);

            var bytes = await File.ReadAllBytesAsync(tmpGz, ct);
            var fileName = $"db-{stamp}.sql.gz";

            TryDelete(tmpGz);

            _log.LogInformation("DB BACKUP finished in {Elapsed} ms. File={File}", sw.ElapsedMilliseconds, fileName);
            return new DatabaseBackupResponse(bytes, fileName);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "DB BACKUP failed.");
            throw;
        }
        finally
        {
            Gate.Release();
        }
    }

    public async Task RestoreAsync(Stream fileStream, string fileName, CancellationToken ct)
    {
        await Gate.WaitAsync(ct);
        var swTotal = Stopwatch.StartNew();

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var safeName = MakeSafeFileName(fileName) ?? $"{_prefix}-{stamp}.sql";
        var tmpPath = Path.Combine(Path.GetTempPath(), safeName);

        try
        {
            _log.LogInformation("DB RESTORE started. Incoming file={Name}", fileName);

            await using (var fs = File.Create(tmpPath))
                await fileStream.CopyToAsync(fs, ct);

            _log.LogInformation("Uploaded saved to {Tmp}. Size={Size} bytes", tmpPath, new FileInfo(tmpPath).Length);

            await TryAutoBackupAsync(stamp, ct);

            var isGzip = tmpPath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
            var sqlPath = isGzip ? await DecompressAsync(tmpPath, ct) : tmpPath;

            _log.LogInformation("Importing SQL via MySqlBackup...");

            await ImportFromSqlAsync(sqlPath, ct);

            if (_runMigrations)
            {
                _log.LogInformation("Running EF migrations...");
                await _db.Database.MigrateAsync(ct);
                _log.LogInformation("EF migrations completed.");
            }

            if (isGzip) TryDelete(sqlPath);
            TryDelete(tmpPath);

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

    private async Task ExportToSqlAsync(string sqlPath, CancellationToken ct)
    {
        await using var conn = new MySqlConnection(_connString);
        await conn.OpenAsync(ct);

        using var cmd = new MySqlCommand { Connection = conn, CommandTimeout = 0 };
        using var mb = new MySqlBackup(cmd);

        mb.ExportInfo.AddCreateDatabase = true;
        mb.ExportInfo.AddDropDatabase = false;
        mb.ExportInfo.AddDropTable = true;
        mb.ExportInfo.ExportFunctions = true;
        mb.ExportInfo.ExportProcedures = true;
        mb.ExportInfo.ExportTriggers = true;
        mb.ExportInfo.ExportViews = true;
        mb.ExportInfo.ExportEvents = true;
        mb.ExportInfo.ResetAutoIncrement = false;
        mb.ExportInfo.RecordDumpTime = true;

        mb.ExportToFile(sqlPath);
    }

    private static async Task CompressGzipAsync(string inputPath, string outputPath, CancellationToken ct)
    {
        await using var input = File.OpenRead(inputPath);
        await using var output = File.Create(outputPath);
        await using var gzip = new GZipStream(output, CompressionLevel.Optimal);
        await input.CopyToAsync(gzip, 1024 * 1024, ct);
    }

    private async Task<string> DecompressAsync(string gzPath, CancellationToken ct)
    {
        var sqlPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(gzPath));

        await using var input = File.OpenRead(gzPath);
        await using var gz = new GZipStream(input, CompressionMode.Decompress);
        await using var outFs = File.Create(sqlPath);
        await gz.CopyToAsync(outFs, ct);

        _log.LogInformation("Decompressed to {Sql}. Size={Size} bytes", sqlPath, new FileInfo(sqlPath).Length);
        return sqlPath;
    }

    private async Task ImportFromSqlAsync(string sqlPath, CancellationToken ct)
    {
        await using var conn = new MySqlConnection(_connString);
        await conn.OpenAsync(ct);

        using var cmd = new MySqlCommand { Connection = conn, CommandTimeout = 0 };
        using var mb = new MySqlBackup(cmd);

        var errLog = Path.Combine(_backupDir, $"import-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log");
        mb.ImportInfo.ErrorLogFile = errLog;
        mb.ImportInfo.IgnoreSqlError = false;

        mb.ImportFromFile(sqlPath);
        _log.LogInformation("Import completed. SqlErrorLog={ErrLog}", errLog);
    }

    private async Task TryAutoBackupAsync(string stamp, CancellationToken ct)
    {
        try
        {
            var backup = await CreateBackupInternalAsync(stamp, ct);
            var final = Path.Combine(_backupDir, $"{_prefix}-AUTO-{stamp}.sql.gz");

            File.Move(backup, final, overwrite: true);
            RotateBackups();

            _log.LogInformation("Auto-backup saved to {Dest}", final);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Auto-backup failed. Continue restore anyway.");
        }
    }

    // Внутренний бэкап в файл (для авто-бэкапа и ротации), без отдачи байтов наружу.
    private async Task<string> CreateBackupInternalAsync(string stamp, CancellationToken ct)
    {
        var tmpSql = Path.Combine(Path.GetTempPath(), $"{_prefix}-{stamp}.sql");
        var tmpGz = Path.Combine(Path.GetTempPath(), $"{_prefix}-{stamp}.sql.gz");

        await ExportToSqlAsync(tmpSql, ct);
        await CompressGzipAsync(tmpSql, tmpGz, ct);
        TryDelete(tmpSql);

        return tmpGz;
    }

    private void RotateBackups()
    {
        if (_keepLocal <= 0) return;

        var files = Directory.GetFiles(_backupDir, $"{_prefix}-AUTO-*.sql.gz")
            .OrderByDescending(f => f)
            .ToList();

        if (files.Count <= _keepLocal) return;

        foreach (var f in files.Skip(_keepLocal))
            TryDelete(f);
    }

    private static string? MakeSafeFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = Path.GetFileName(name);
        return Regex.Replace(name, @"[^a-zA-Z0-9_\-\.]+", "_");
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch
        {
            /* ignore */
        }
    }
}