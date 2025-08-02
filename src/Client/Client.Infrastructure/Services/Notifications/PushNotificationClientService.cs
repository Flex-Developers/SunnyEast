using Application.Contract.NotificationSubscriptions;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Client.Infrastructure.Services.Notifications;

public interface IPushNotificationClientService
{
    Task<bool> InitializeAsync();
    Task<CreateNotificationSubscriptionCommand?> SubscribeAsync();
    Task<bool> UnsubscribeAsync();
    Task<bool> IsEnabledAsync();
    Task<bool> RequestPermissionAsync();
    Task<CreateNotificationSubscriptionCommand?> GetSubscriptionAsync();
}

public class PushNotificationClientService(
    IJSRuntime jsRuntime,
    ILogger<PushNotificationClientService> logger,
    IConfiguration configuration)
    : IPushNotificationClientService
{
    private string? _vapidPublicKey;

    public async Task<bool> InitializeAsync()
    {
        try
        {
            _vapidPublicKey = configuration["PushNotifications:VapidPublicKey"];

            if (string.IsNullOrEmpty(_vapidPublicKey))
            {
                logger.LogWarning("VAPID public key not configured");
                return false;
            }

            logger.LogInformation("Push notifications initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize push notifications");
            return false;
        }
    }

    public async Task<bool> UnsubscribeAsync()
    {
        try
        {
            var endpoint = await jsRuntime.InvokeAsync<string?>("pushInterop.unsubscribeUser");
            var success = !string.IsNullOrEmpty(endpoint);

            logger.LogInformation("Unsubscribe result: {Success}, Endpoint: {Endpoint}", success, endpoint);
            return success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unsubscribe from push notifications");
            return false;
        }
    }

    public async Task<CreateNotificationSubscriptionCommand?> SubscribeAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_vapidPublicKey))
            {
                logger.LogWarning("VAPID public key not configured");
                return null;
            }

            var createCommand =
                await jsRuntime.InvokeAsync<CreateNotificationSubscriptionCommand?>(
                    "pushInterop.subscribeUser",
                    _vapidPublicKey);
            Console.WriteLine(createCommand); //todo remove this shit
            return createCommand;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unsubscribe from push notifications");
        }

        return null;
    }

    public async Task<bool> IsEnabledAsync()
    {
        try
        {
            var permission = await GetSubscriptionAsync();
            return permission != null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check notification status");
            return false;
        }
    }

    public async Task<bool> RequestPermissionAsync()
    {
        try
        {
            var permission = await jsRuntime.InvokeAsync<string>("pushInterop.requestNotificationPermission");
            return permission == "granted";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check notification status");
            return false;
        }
    }

    public async Task<CreateNotificationSubscriptionCommand?> GetSubscriptionAsync()
    {
        return await jsRuntime.InvokeAsync<CreateNotificationSubscriptionCommand?>("pushInterop.getSubscription");
    }
}