using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Client.Pages.Admin.Media;

public partial class ImageUpload
{
    private IReadOnlyList<IBrowserFile> _files = new List<IBrowserFile>();
    private readonly Dictionary<string, string> _uploadedFiles = new();         // текущее окно
    private readonly Dictionary<string, double> _uploadProgress = new();
    private bool _isUploading;
    private string _cdnApiBaseUrl = string.Empty;
    private const long MaxFileSize = 10L * 1024 * 1024; // 10MB

    private const string LocalStorageKey = "se_uploaded_images";
    private List<UploadedImageInfo> _gallery = new();   // постоянный список (LocalStorage)

    private record UploadedImageInfo(string Name, string Url, DateTime UploadedAt);

    protected override void OnInitialized()
    {
        _cdnApiBaseUrl = Configuration.GetSection("CdnApi:BaseUrl").Value ?? "http://localhost:5000";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadLocalGallery();
            // для правой колонки: восстановим имена->url (без повторных загрузок)
            foreach (var img in _gallery)
                if (!_uploadedFiles.ContainsKey(img.Name))
                    _uploadedFiles[img.Name] = img.Url;
            await InvokeAsync(StateHasChanged);
        }
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

            using var stream = file.OpenReadStream(MaxFileSize);
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            _uploadProgress[file.Name] = 50;
            await InvokeAsync(StateHasChanged);

            // ВАЖНО: этот вызов ориентирован на Ваш CDN API.
            // Если используете свой WebApi контроллер ниже — скорректируйте маршрут.
            var response = await HttpClient.PostAsync($"{_cdnApiBaseUrl}/api/Images/upload", content);

            if (response.Success)
            {
                _uploadProgress[file.Name] = 100;
                var responseContent = await response.Response!.Content.ReadAsStringAsync();
                // Ожидаем JSON вида: { "url": "https://.../name.ext" }
                var url = JsonDocument.Parse(responseContent).RootElement.GetProperty("url").GetString();

                if (!string.IsNullOrWhiteSpace(url))
                {
                    _uploadedFiles[file.Name] = url!;
                    UpsertToLocalGallery(file.Name, url!);
                    await SaveLocalGallery();

                    Snackbar.Add($"Файл {file.Name} успешно загружен", Severity.Success);
                }
                else
                {
                    Snackbar.Add($"Сервер вернул пустой URL для {file.Name}", Severity.Warning);
                }
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
        => _uploadProgress.TryGetValue(file.Name, out var progress) ? progress : 0;

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
        catch
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

    // ======= LocalStorage =======

    private async Task LoadLocalGallery()
    {
        try
        {
            var json = await JSRuntime.InvokeAsync<string>("localStorage.getItem", LocalStorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var list = JsonSerializer.Deserialize<List<UploadedImageInfo>>(json);
                if (list is not null) _gallery = list;
            }
        }
        catch
        {
            // игнорируем — не критично
        }
    }

    private async Task SaveLocalGallery()
    {
        try
        {
            var json = JsonSerializer.Serialize(_gallery);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, json);
        }
        catch
        {
            Snackbar.Add("Не удалось сохранить список изображений в браузере", Severity.Warning);
        }
    }

    private void UpsertToLocalGallery(string name, string url)
    {
        var existing = _gallery.FirstOrDefault(x => x.Name == name);
        if (existing is not null)
        {
            _gallery.Remove(existing);
        }
        _gallery.Insert(0, new UploadedImageInfo(name, url, DateTime.UtcNow));
    }

    private async Task RemoveFromLocalGallery(UploadedImageInfo img)
    {
        _gallery.RemoveAll(x => x.Name == img.Name && x.Url == img.Url);
        await SaveLocalGallery();
        Snackbar.Add("Изображение удалено из локального списка", Severity.Info);
        await InvokeAsync(StateHasChanged);
    }

    private async Task ClearLocalGallery()
    {
        var confirm = await Dialog.ShowMessageBox(
            "Очистить список",
            "Удалить все записи только из локального списка? Файлы на сервере останутся.",
            yesText: "Да", cancelText: "Отмена");
        if (confirm == true)
        {
            _gallery.Clear();
            await JSRuntime.InvokeVoidAsync("localStorage.removeItem", LocalStorageKey);
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task CopyAllUrls()
    {
        if (!_gallery.Any()) return;
        var text = string.Join(Environment.NewLine, _gallery.Select(g => g.Url));
        await CopyUrlToClipboard(text);
    }

    // ======= Delete on server (опционально, если есть DELETE API) =======

    private async Task DeleteOnServer(UploadedImageInfo img)
    {
        var confirm = await Dialog.ShowMessageBox(
            "Удалить на сервере",
            "Удалить этот файл на сервере? Отменить будет нельзя.",
            yesText: "Удалить", cancelText: "Отмена");

        if (confirm != true) 
            return;

        try
        {
            var fileName = ExtractFileName(img.Url);
            // Ожидаемый маршрут: DELETE {BaseUrl}/api/Images/{fileName}
            Console.WriteLine(fileName);
            Console.WriteLine(WebUtility.UrlEncode(fileName));
            Console.WriteLine($"{_cdnApiBaseUrl}/api/Images/{WebUtility.UrlEncode(fileName)}");
            var resp = await HttpClient.DeleteAsync($"{_cdnApiBaseUrl}/api/Images/{WebUtility.UrlEncode(fileName)}");

            if (resp.Success)
            {
                await RemoveFromLocalGallery(img);
                _uploadedFiles.Remove(img.Name);
                Snackbar.Add("Файл удалён на сервере", Severity.Success);
            }
            else
            {
                Snackbar.Add($"Сервер отклонил удаление: {resp.StatusCode}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Ошибка удаления на сервере: {ex.Message}", Severity.Error);
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private static string ExtractFileName(string url)
    {
        // Работает и с CDN-ссылками: берём последний сегмент пути
        try
        {
            var uri = new Uri(url);
            return Path.GetFileName(uri.LocalPath);
        }
        catch
        {
            // На случай, если это невалидный URI — берём хвост после последнего '/'
            var idx = url.LastIndexOf('/');
            return idx >= 0 ? url[(idx + 1)..] : url;
        }
    }
}
