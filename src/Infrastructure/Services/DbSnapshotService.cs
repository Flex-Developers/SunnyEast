namespace Infrastructure.Services;

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Contract.Database.Responses;
using Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

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

    // Каталог с бинарями pg_dump/psql (если они не в PATH). По умолчанию вызываем по имени.
    private readonly string _pgBinDir;

    public DbSnapshotService(IConfiguration cfg, ApplicationDbContext db, ILogger<DbSnapshotService> log)
    {
        _cfg = cfg;
        _db = db;
        _log = log;

        _connString = cfg.GetConnectionString("Postgres")
                      ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");

        _backupDir = _cfg["Backups:Directory"] ?? "App_Data/Backups";
        _prefix = _cfg["Backups:FilePrefix"] ?? "snapshot";
        _runMigrations = bool.TryParse(_cfg["Backups:RunMigrationsAfterRestore"], out var b) && b;
        _keepLocal = int.TryParse(_cfg["Backups:KeepLastLocalCopies"], out var k) ? Math.Max(0, k) : 10;
        _pgBinDir = _cfg["Backups:PgBinDir"] ?? string.Empty;

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

            _log.LogInformation("Importing SQL via psql...");

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
        var csb = new NpgsqlConnectionStringBuilder(_connString);

        // pg_dump: чистый plain-SQL дамп со сбросом объектов (--clean --if-exists),
        // без владельцев/привилегий — чтобы restore не зависел от ролей целевого сервера.
        await RunPgToolAsync("pg_dump", csb, args =>
        {
            args.Add("--no-owner");
            args.Add("--no-privileges");
            args.Add("--clean");
            args.Add("--if-exists");
            args.Add("--format=plain");
            args.Add("--encoding=UTF8");
            args.Add($"--file={sqlPath}");
        }, ct);
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
        var csb = new NpgsqlConnectionStringBuilder(_connString);
        var errLog = Path.Combine(_backupDir, $"import-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log");

        // psql: останавливаемся на первой ошибке (аналог IgnoreSqlError=false)
        // и накатываем весь дамп одной транзакцией.
        var stderr = await RunPgToolAsync("psql", csb, args =>
        {
            args.Add("--set=ON_ERROR_STOP=1");
            args.Add("--single-transaction");
            args.Add($"--file={sqlPath}");
        }, ct);

        if (!string.IsNullOrWhiteSpace(stderr))
            await File.WriteAllTextAsync(errLog, stderr, ct);

        _log.LogInformation("Import completed. SqlErrorLog={ErrLog}", errLog);
    }

    /// <summary>
    /// Запускает pg_dump/psql с параметрами подключения из строки Npgsql.
    /// Пароль передаётся через переменную окружения PGPASSWORD. Возвращает stderr процесса.
    /// </summary>
    private async Task<string> RunPgToolAsync(string tool, NpgsqlConnectionStringBuilder csb,
        Action<System.Collections.ObjectModel.Collection<string>> addArgs, CancellationToken ct)
    {
        var fileName = string.IsNullOrEmpty(_pgBinDir) ? tool : Path.Combine(_pgBinDir, tool);

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        psi.ArgumentList.Add($"--host={csb.Host ?? "localhost"}");
        psi.ArgumentList.Add($"--port={(csb.Port == 0 ? 5432 : csb.Port)}");
        psi.ArgumentList.Add($"--username={csb.Username ?? "postgres"}");
        psi.ArgumentList.Add($"--dbname={csb.Database}");
        psi.ArgumentList.Add("--no-password");
        addArgs(psi.ArgumentList);

        psi.Environment["PGPASSWORD"] = csb.Password ?? string.Empty;

        using var proc = new Process { StartInfo = psi };

        var stdErr = new StringBuilder();
        proc.ErrorDataReceived += (_, e) => { if (e.Data != null) stdErr.AppendLine(e.Data); };
        proc.OutputDataReceived += (_, _) => { /* drain stdout to avoid blocking */ };

        try
        {
            proc.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to start '{fileName}'. Ensure PostgreSQL client tools are installed and on PATH " +
                "(or set Backups:PgBinDir).", ex);
        }

        proc.BeginErrorReadLine();
        proc.BeginOutputReadLine();

        await proc.WaitForExitAsync(ct);

        if (proc.ExitCode != 0)
            throw new InvalidOperationException(
                $"{tool} exited with code {proc.ExitCode}. {stdErr}".Trim());

        return stdErr.ToString();
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
