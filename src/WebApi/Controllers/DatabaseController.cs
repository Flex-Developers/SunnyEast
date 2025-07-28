using Application.Contract.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services.DbSnapshot;

namespace WebApi.Controllers;

[Authorize(Roles = ApplicationRoles.SuperAdmin)]
public sealed class DatabaseController(IDbSnapshotService snapshots, ILogger<DatabaseController> logger) : ApiControllerBase
{
    /// <summary>Скачать полный снимок БД (.sql.gz)</summary>
    [HttpGet("backup")]
    public async Task<IActionResult> Backup(CancellationToken ct)
    {
        var gzPath = await snapshots.CreateBackupGzipAsync(ct);

        var fileName = $"db-{DateTime.UtcNow:yyyyMMdd-HHmmss}.sql.gz";
        var stream = System.IO.File.OpenRead(gzPath);

        // удаляем временный файл после отдачи
        HttpContext.Response.RegisterForDisposeAsync(new AsyncDisposableAction(async () =>
        {
            try { await stream.DisposeAsync(); } catch { }
            try { System.IO.File.Delete(gzPath); } catch { /* ignore */ }
            await Task.CompletedTask;
        }));

        return File(stream, "application/gzip", fileName, enableRangeProcessing: true);
    }

    /// <summary>Загрузить и восстановить БД из файла (.sql или .sql.gz).</summary>
    [RequestSizeLimit(long.MaxValue)]
    [HttpPost("restore")]
    public async Task<IActionResult> Restore([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не загружен.");

        var allowed = new[] { ".sql", ".gz" };
        var ext = Path.GetExtension(file.FileName);
        if (!allowed.Contains(ext, StringComparer.OrdinalIgnoreCase))
            return BadRequest("Поддерживаются только .sql и .sql.gz");

        await using var stream = file.OpenReadStream();
        try
        {
            await snapshots.RestoreFromDumpAsync(stream, file.FileName, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка восстановления БД");
            return StatusCode(500, $"Ошибка восстановления: {ex.Message}");
        }

        return Ok(new { Message = "База данных успешно восстановлена." });
    }

    private sealed class AsyncDisposableAction(Func<ValueTask> action) : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => action();
    }
}