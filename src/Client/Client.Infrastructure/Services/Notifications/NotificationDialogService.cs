using MudBlazor;

namespace Client.Infrastructure.Services.Notifications;

public interface INotificationDialogService
{
    Task<bool> ShowPermissionDialogAsync();
}

public class NotificationDialogService(IDialogService dialogService) : INotificationDialogService
{
    public async Task<bool> ShowPermissionDialogAsync()
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.Small };
        var dialog = await dialogService.ShowAsync<NotificationPermissionDialog>("", options);
        var result = await dialog.Result;
        return !result.Canceled && result.Data is true;
    }
}
