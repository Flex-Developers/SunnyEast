using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Client.Pages.Admin.Media;

public partial class ImageUpload
{
    private IReadOnlyList<IBrowserFile> _files = new List<IBrowserFile>();
    private readonly Dictionary<string, string> _uploadedFiles = new();
    private readonly Dictionary<string, double> _uploadProgress = new();
    private bool _isUploading;
    private string _cdnApiBaseUrl = string.Empty;
    private const long MaxFileSize = 10L * 1024 * 1024; // 10MB

    protected override void OnInitialized()
    {
        _cdnApiBaseUrl = Configuration.GetSection("CdnApi:BaseUrl").Value ?? "http://localhost:5000";
    }

    private async Task OnInputFileChanged(InputFileChangeEventArgs e)
    {
        var fileList = new List<IBrowserFile>();

        var selectedFiles = e.GetMultipleFiles(10);
        foreach (var file in selectedFiles)
        {
            if (IsValidImageFile(file))
            {
                fileList.Add(file);
            }
            else
            {
                Snackbar.Add($"Файл {file.Name} не поддерживается или слишком большой", Severity.Warning);
            }
        }

        _files = fileList;
        await InvokeAsync(StateHasChanged);
    }

    private bool IsValidImageFile(IBrowserFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        return allowedTypes.Contains(file.ContentType.ToLower()) && file.Size <= MaxFileSize;
    }

    private async Task UploadFiles()
    {
        if (!_files.Any() || _isUploading) return;

        _isUploading = true;
        await InvokeAsync(StateHasChanged);

        var uploadTasks = _files.Select(UploadSingleFile);
        await Task.WhenAll(uploadTasks);

        _isUploading = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UploadSingleFile(IBrowserFile file)
    {
        try
        {
            _uploadProgress[file.Name] = 0;
            await InvokeAsync(StateHasChanged);

            using var content = new MultipartFormDataContent();

            // Use maxAllowedSize parameter for WASM
            using var stream = file.OpenReadStream(MaxFileSize);
            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            _uploadProgress[file.Name] = 50;
            await InvokeAsync(StateHasChanged);

            var response = await HttpClient.PostAsync($"{_cdnApiBaseUrl}/api/Images/upload", content);

            if (response.Success)
            {
                _uploadProgress[file.Name] = 100;
                var imageUrl = $"{_cdnApiBaseUrl}/api/Images/{file.Name}";
                _uploadedFiles[file.Name] = imageUrl;
                Snackbar.Add($"Файл {file.Name} успешно загружен", Severity.Success);
            }
            else
            {
                Snackbar.Add($"Ошибка загрузки {file.Name}: {response.StatusCode}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Ошибка загрузки {file.Name}: {ex.Message}", Severity.Error);
            _uploadProgress.Remove(file.Name);
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ClearFiles()
    {
        _files = new List<IBrowserFile>();
        _uploadProgress.Clear();
        await InvokeAsync(StateHasChanged);
    }

    private double GetUploadProgress(IBrowserFile file)
    {
        return _uploadProgress.TryGetValue(file.Name, out var progress) ? progress : 0;
    }

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;

        while (Math.Round(number / 1024) >= 1 && counter < suffixes.Length - 1)
        {
            number /= 1024;
            counter++;
        }

        return $"{number:n1}{suffixes[counter]}";
    }

    private async Task CopyUrlToClipboard(string url)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", url);
            Snackbar.Add("Ссылка скопирована в буфер обмена", Severity.Success);
        }
        catch (Exception)
        {
            Snackbar.Add("Не удалось скопировать ссылку", Severity.Error);
        }
    }

    private async Task RemoveFile(IBrowserFile file)
    {
        var fileList = _files.ToList();
        fileList.Remove(file);
        _files = fileList;
        _uploadProgress.Remove(file.Name);
        await InvokeAsync(StateHasChanged);
    }

    private async Task FilesChangedHandler(IReadOnlyList<IBrowserFile> files)
    {
        _files = files;
        await InvokeAsync(StateHasChanged);
    }
}