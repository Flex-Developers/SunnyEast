using Microsoft.Extensions.Logging;
using Client.Infrastructure.Services.Notifications;

namespace Client.Infrastructure.Services.Notifications;

public interface INotificationManager
{
    Task InitializeAsync();
    Task<bool> EnableNotificationsAsync();
    Task<bool> DisableNotificationsAsync();
    Task<bool> IsEnabledAsync();
    string GetPermissionStatus();
    event EventHandler<NotificationStatusChangedEventArgs>? StatusChanged;
}

public class NotificationStatusChangedEventArgs : EventArgs
{
    public bool IsEnabled { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class NotificationManager : INotificationManager
{
    private readonly IPushNotificationClientService _clientService;
    private readonly INotificationSubscriptionApiService _apiService;
    private readonly ILogger<NotificationManager> _logger;
    private bool _isInitialized;

    public event EventHandler<NotificationStatusChangedEventArgs>? StatusChanged;

    public NotificationManager(
        IPushNotificationClientService clientService,
        INotificationSubscriptionApiService apiService,
        ILogger<NotificationManager> logger)
    {
        _clientService = clientService;
        _apiService = apiService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        try
        {
            _logger.LogInformation("Initializing notification manager...");
            
            var success = await _clientService.InitializeAsync();
            _isInitialized = success;

            if (!success)
            {
                _logger.LogWarning("Failed to initialize push notification client service");
                return;
            }

            // Check current status and sync with server if needed
            await SyncSubscriptionStatusAsync();
            
            _logger.LogInformation("Notification manager initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize notification manager");
        }
    }

    public async Task<bool> EnableNotificationsAsync()
    {
        if (!_isInitialized)
        {
            _logger.LogWarning("Notification manager not initialized");
            return false;
        }

        try
        {
            _logger.LogInformation("Enabling push notifications...");

            // Subscribe to push notifications
            var subscriptionResult = await _clientService.SubscribeAsync();
            
            if (!subscriptionResult.Success)
            {
                _logger.LogWarning("Failed to subscribe to push notifications: {Reason}", subscriptionResult.Reason);
                
                OnStatusChanged(false, subscriptionResult.Reason ?? "subscription-failed", subscriptionResult.Error);
                return false;
            }

            // If we have a subscription, send it to the server
            if (subscriptionResult.Subscription != null)
            {
                var serverSuccess = await _apiService.CreateSubscriptionAsync(subscriptionResult.Subscription);
                
                if (!serverSuccess)
                {
                    _logger.LogWarning("Failed to register subscription with server");
                    // Don't return false here - the client subscription still works
                }
                else
                {
                    _logger.LogInformation("Successfully registered subscription with server");
                }
            }

            OnStatusChanged(true, "granted", null);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable push notifications");
            OnStatusChanged(false, "error", ex.Message);
            return false;
        }
    }

    public async Task<bool> DisableNotificationsAsync()
    {
        if (!_isInitialized)
        {
            return false;
        }

        try
        {
            _logger.LogInformation("Disabling push notifications...");

            // First get the current subscription to get the endpoint
            var subscription = await _clientService.GetSubscriptionAsync();
            string? endpoint = null;
            
            if (subscription != null && subscription.Success && subscription.Subscription != null)
            {
                endpoint = subscription.Subscription.Endpoint;
            }

            // Unsubscribe from client
            var unsubscribeSuccess = await _clientService.UnsubscribeAsync();
            
            // Remove from server if we have an endpoint
            if (!string.IsNullOrEmpty(endpoint))
            {
                await _apiService.DeleteSubscriptionAsync(endpoint);
            }

            OnStatusChanged(false, "disabled", null);
            return unsubscribeSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable push notifications");
            return false;
        }
    }

    public async Task<bool> IsEnabledAsync()
    {
        if (!_isInitialized)
            return false;

        return await _clientService.IsEnabledAsync();
    }

    public string GetPermissionStatus()
    {
        if (!_isInitialized)
            return "not-initialized";

        return _clientService.GetPermissionStatus();
    }

    private async Task SyncSubscriptionStatusAsync()
    {
        try
        {
            var isEnabled = await _clientService.IsEnabledAsync();
            var status = _clientService.GetPermissionStatus();
            
            _logger.LogInformation("Current notification status: {Status}, Enabled: {IsEnabled}", status, isEnabled);
            
            // If notifications are enabled but we don't have a server subscription, create one
            if (isEnabled && status == "granted")
            {
                // We could implement a check to see if the server has our subscription
                // For now, we'll just ensure the status is properly reported
                OnStatusChanged(true, status, null);
            }
            else
            {
                OnStatusChanged(false, status, null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync subscription status");
        }
    }

    private void OnStatusChanged(bool isEnabled, string status, string? reason)
    {
        StatusChanged?.Invoke(this, new NotificationStatusChangedEventArgs
        {
            IsEnabled = isEnabled,
            Status = status,
            Reason = reason
        });
    }
}
