using Microsoft.Extensions.Logging;

namespace Client.Infrastructure.Services.Notifications;

public interface INotificationManager
{
    Task InitializeAsync();
    Task<bool> EnableNotificationsAsync();
    Task<bool> DisableNotificationsAsync();
    Task<bool> IsEnabledAsync();
    Task<bool> RequestPermissionWithDialogAsync();
}

public class NotificationManager(
    IPushNotificationClientService clientService,
    INotificationSubscriptionApiService apiService,
    INotificationDialogService dialogService,
    ILogger<NotificationManager> logger)
    : INotificationManager
{
    private bool _isInitialized;

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        try
        {
            logger.LogInformation("Initializing notification manager...");

            var success = await clientService.InitializeAsync();
            _isInitialized = success;

            if (!success)
            {
                logger.LogWarning("Failed to initialize push notification client service");
                return;
            }

            // Check current status and sync with server if needed
            await SyncSubscriptionStatusAsync();

            logger.LogInformation("Notification manager initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize notification manager");
        }
    }

    public async Task<bool> EnableNotificationsAsync()
    {
        if (!_isInitialized)
        {
            await InitializeAsync();
            logger.LogWarning("Notification manager not initialized");
        }

        try
        {
            logger.LogInformation("Enabling push notifications...");

            var requestPermissionResult = await clientService.RequestPermissionAsync();
            if (!requestPermissionResult)
            {
                logger.LogWarning("User denied push notification permission");
                return false;
            }

            var subscriptionResult = await clientService.SubscribeAsync();

            if (subscriptionResult != null)
            {
                var serverSuccess = await apiService.CreateSubscriptionAsync(subscriptionResult);

                if (!serverSuccess)
                {
                    logger.LogWarning("Failed to register subscription with server");
                    // Don't return false here - the client subscription still works
                }
                else
                {
                    logger.LogInformation("Successfully registered subscription with server");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enable push notifications");
            return false;
        }
    }

    public async Task<bool> DisableNotificationsAsync()
    {
        if (!_isInitialized)
        {
            await InitializeAsync();
        }

        try
        {
            logger.LogInformation("Disabling push notifications...");


            var unsubscribeEndpoint = await clientService.UnsubscribeAsync();
            if (unsubscribeEndpoint != null)
            {
                return await apiService.DeleteSubscriptionAsync(unsubscribeEndpoint);
            }


            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to disable push notifications");
            return false;
        }
    }

    public async Task<bool> IsEnabledAsync()
    {
        if (!_isInitialized)
            await InitializeAsync();

        return await clientService.IsEnabledAsync();
    }

    public async Task<bool> RequestPermissionWithDialogAsync()
    {
        if (!_isInitialized)
        {
            await InitializeAsync();
            if (!_isInitialized)
            {
                logger.LogWarning("Notification manager not initialized");
                return false;
            }
        }

        try
        {
            // Show dialog to get user consent first
            var userConsent = await dialogService.ShowPermissionDialogAsync();
            if (!userConsent)
            {
                logger.LogInformation("User declined notification permission dialog");
                return false;
            }

            // Request permission immediately after user clicks "Allow" 
            // This ensures it's within the user-generated event handler
            logger.LogInformation("User agreed, requesting browser permission...");
            var requestPermissionResult = await clientService.RequestPermissionAsync();
            if (!requestPermissionResult)
            {
                logger.LogWarning("Browser denied push notification permission");
                return false;
            }

            // Subscribe and register with server
            var subscriptionResult = await clientService.SubscribeAsync();
            if (subscriptionResult != null)
            {
                var serverSuccess = await apiService.CreateSubscriptionAsync(subscriptionResult);
                if (serverSuccess)
                {
                    logger.LogInformation("Successfully registered subscription with server");
                }
                else
                {
                    logger.LogWarning("Failed to register subscription with server");
                }
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enable push notifications with dialog");
            return false;
        }
    }

    private async Task SyncSubscriptionStatusAsync()
    {
        try
        {
            var isEnabled = await clientService.IsEnabledAsync();
            var status = await clientService.GetSubscriptionAsync();

            logger.LogInformation("Current notification status: {Status}, Enabled: {IsEnabled}", status, isEnabled);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync subscription status");
        }
    }
}