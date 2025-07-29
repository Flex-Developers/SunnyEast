namespace WebApi.Controllers;

using Application.Contract.Database.Commands;
using Application.Contract.Database.Queries;
using Application.Contract.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = ApplicationRoles.SuperAdmin)]
public sealed class DatabaseController : ApiControllerBase
{
    /// <summary>Скачать полный снимок БД (.sql.gz)</summary>
    [HttpGet("backup")]
    public async Task<IActionResult> Backup(CancellationToken ct)
    {
        var file = await Mediator.Send(new DownloadDatabaseBackupQuery(), ct);
        return File(file.Content, file.ContentType, file.FileName);
    }

    /// <summary>Загрузить и восстановить БД из файла (.sql или .sql.gz)</summary>
    [RequestSizeLimit(long.MaxValue)]
    [HttpPost("restore")]
    public async Task<IActionResult> Restore([FromForm] IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        await Mediator.Send(new RestoreDatabaseCommand(stream, file.FileName), ct);
        return Ok(new { Message = "База данных успешно восстановлена." });
    }
}