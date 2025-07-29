using Microsoft.JSInterop;
using Client.Infrastructure.Services.HttpClient;
using Microsoft.AspNetCore.Components.Forms;

namespace Client.Infrastructure.Services.Database;

public sealed class DatabaseAdminService(IHttpClientService httpClient,  IJSRuntime jsRuntime) : IDatabaseAdminService
{
    public async Task<bool> DownloadAsync()
    {
        // К API обращаемся ОТНОСИТЕЛЬНЫМ путём — ваш HttpClientService сам добавит baseUrl и заголовок Authorization
        var res = await httpClient.GetBytesAsync("/api/database/backup");
        if (!res.Success || res.Response is null)
            return false;

        // сохраняем файл на клиенте
        var fileName = $"db-{DateTime.UtcNow:yyyyMMdd-HHmmss}.sql.gz";
        await jsRuntime.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/gzip", res.Response);
        return true;
    }

    public async Task<bool> RestoreAsync(IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();

        await using var stream = file.OpenReadStream(long.MaxValue);
        var fileContent = new StreamContent(stream);

        var ext = Path.GetExtension(file.Name).ToLowerInvariant();
        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(ext == ".gz" ? "application/gzip" : "application/sql");

        content.Add(fileContent, "file", file.Name);

        // Стало: даём, например, до 30 минут
        var response = await httpClient.PostAsync("/api/database/restore", content);
        return response.Success;
    }

}