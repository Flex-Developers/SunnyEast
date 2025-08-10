﻿using Application.Contract.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize(Roles = ApplicationRoles.Administrator)]
[Route("api/[controller]")]
public class ImageController : ApiControllerBase
{
    private static string RootFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sunnyEastImages");

    public ImageController()
    {
        Directory.CreateDirectory(RootFolder);
    }

    /// <summary>
    /// Загрузка изображения в base64 (совместимость со старым клиентом).
    /// </summary>
    [HttpPost] // POST api/Image
    public async Task<IActionResult> UploadBase64([FromBody] string imageBase64, [FromQuery] string imageType)
    {
        if (string.IsNullOrWhiteSpace(imageBase64) || string.IsNullOrWhiteSpace(imageType))
            return BadRequest("imageBase64 и imageType обязательны");

        var bytes = Convert.FromBase64String(imageBase64);
        var imageName = $"{Guid.NewGuid():N}.{imageType.Trim().TrimStart('.')}";

        var path = Path.Combine(RootFolder, imageName);
        await System.IO.File.WriteAllBytesAsync(path, bytes);

        var url = $"/api/Image/{imageName}";
        return Ok(new { url, name = imageName });
    }

    /// <summary>
    /// Загрузка через multipart/form-data (современный кейс).
    /// </summary>
    [HttpPost("upload")] // POST api/Image/upload
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadMultipart([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("Файл не найден");

        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var allowed = new[] { "jpg", "jpeg", "png", "gif", "webp" };
        if (!allowed.Contains(ext)) return BadRequest("Неподдерживаемый формат");

        var imageName = $"{Guid.NewGuid():N}.{ext}";
        var path = Path.Combine(RootFolder, imageName);
        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        var url = $"/api/Image/{imageName}";
        return Ok(new { url, name = imageName });
    }

    /// <summary>
    /// Отдать файл по имени.
    /// </summary>
    [HttpGet("{name}")] // GET api/Image/{name}
    public IActionResult Get(string name)
    {
        var path = Path.Combine(RootFolder, name);
        if (!System.IO.File.Exists(path)) return NotFound();

        var mime = GetMime(Path.GetExtension(name));
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, mime ?? "application/octet-stream", fileDownloadName: name);
    }

    /// <summary>
    /// Удалить файл по имени.
    /// </summary>
    [HttpDelete("{name}")] // DELETE api/Image/{name}
    public IActionResult Delete(string name)
    {
        var path = Path.Combine(RootFolder, name);
        if (!System.IO.File.Exists(path)) return NotFound();

        System.IO.File.Delete(path);
        return NoContent();
    }

    private static string? GetMime(string ext)
    {
        return ext.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => null
        };
    }
}
