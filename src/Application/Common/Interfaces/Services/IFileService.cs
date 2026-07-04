using Application.Contract.File.Responses;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces.Services;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file);
    Task<FileResult?> GetFileAsync(string fileName);
    Task<bool> DeleteFileAsync(string fileName);
    bool FileExists(string fileName);
}