using Application.Common.Interfaces.Services;
using Application.Contract.File.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class FileService(IWebHostEnvironment webHostEnvironment) : IFileService
{
    private readonly string _storagePath = Path.Combine(webHostEnvironment.WebRootPath, "uploads");

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        if (file.Length == 0) throw new ArgumentException("File is empty");

        Directory.CreateDirectory(_storagePath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(_storagePath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }

    public async Task<FileResult?> GetFileAsync(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);

        if (!File.Exists(filePath)) return null;

        var bytes = await File.ReadAllBytesAsync(filePath);
        var contentType = GetContentType(fileName);

        return new FileResult(bytes, contentType, fileName);
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);

        if (!File.Exists(filePath)) return false;

        await Task.Run(() => File.Delete(filePath));
        return true;
    }

    public bool FileExists(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        return File.Exists(filePath);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".bmp" => "image/bmp",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream"
        };
    }
}