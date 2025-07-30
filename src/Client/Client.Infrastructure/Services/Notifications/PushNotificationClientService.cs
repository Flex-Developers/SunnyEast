using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Client.Infrastructure.Services.Notifications;

public interface IPushNotificationClientService
{
    Task<bool> InitializeAsync();
    Task<bool> RequestPermissionAsync();
    Task<NotificationSubscriptionResult> SubscribeAsync();
    Task<NotificationSubscriptionResult> GetSubscriptionAsync();
    Task<bool> UnsubscribeAsync();
    Task<bool> IsEnabledAsync();
    string GetPermissionStatus();
}

public class NotificationSubscriptionResult
{
    public bool Success { get; set; }
    public string? Reason { get; set; }
    public string? Error { get; set; }
    public NotificationSubscription? Subscription { get; set; }
}

public class NotificationSubscription
{
    public string Endpoint { get; set; } = string.Empty;
    public NotificationKeys Keys { get; set; } = new();
}

public class NotificationKeys
{
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}

public class PushNotificationClientService : IPushNotificationClientService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<PushNotificationClientService> _logger;
    private readonly IConfiguration _configuration;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private string? _vapidPublicKey;

    public PushNotificationClientService(
        IJSRuntime jsRuntime, 
        ILogger<PushNotificationClientService> logger,
        IConfiguration configuration)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
        _configuration = configuration;
        _moduleTask = new(() => _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Client.Infrastructure/pushInterop.js").AsTask());
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            _vapidPublicKey = _configuration["PushNotifications:VapidPublicKey"];
            
            if (string.IsNullOrEmpty(_vapidPublicKey))
            {
                _logger.LogWarning("VAPID public key not configured");
                return false;
            }

            var isSupported = await _jsRuntime.InvokeAsync<bool>("pushInterop.isSupported");
            if (!isSupported)
            {
                _logger.LogInformation("Push notifications not supported in this browser");
                return false;
            }

            _logger.LogInformation("Push notifications initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize push notifications");
            return false;
        }
    }

    public async Task<bool> RequestPermissionAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_vapidPublicKey))
            {
                _logger.LogWarning("Cannot request permission - VAPID key not configured");
                return false;
            }

            var permission = await _jsRuntime.InvokeAsync<string>("pushInterop.requestNotificationPermission");
            var success = permission == "granted";
            
            _logger.LogInformation("Notification permission request result: {Permission}", permission);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request notification permission");
            return false;
        }
    }

    public async Task<NotificationSubscriptionResult> SubscribeAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_vapidPublicKey))
            {
                return new NotificationSubscriptionResult
                {
                    Success = false,
                    Reason = "vapid-key-missing",
                    Error = "VAPID public key not configured"
                };
            }

            var result = await _jsRuntime.InvokeAsync<dynamic>("pushInterop.showNotificationPrompt", _vapidPublicKey);
            
            if (result == null)
            {
                return new NotificationSubscriptionResult
                {
                    Success = false,
                    Reason = "subscription-failed",
                    Error = "No result returned from JavaScript"
                };
            }

            // Convert the dynamic result to our typed result
            var success = (bool)result.success;
            
            if (!success)
            {
                return new NotificationSubscriptionResult
                {
                    Success = false,
                    Reason = result.reason?.ToString(),
                    Error = result.error?.ToString()
                };
            }

            var subscription = result.subscription;
            if (subscription != null)
            {
                return new NotificationSubscriptionResult
                {
                    Success = true,
                    Subscription = new NotificationSubscription
                    {
                        Endpoint = subscription.Endpoint?.ToString() ?? "",
                        Keys = new NotificationKeys
                        {
                            P256dh = subscription.Keys?.P256dh?.ToString() ?? "",
                            Auth = subscription.Keys?.Auth?.ToString() ?? ""
                        }
                    }
                };
            }

            return new NotificationSubscriptionResult
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to push notifications");
            return new NotificationSubscriptionResult
            {
                Success = false,
                Reason = "exception",
                Error = ex.Message
            };
        }
    }

    public async Task<NotificationSubscriptionResult> GetSubscriptionAsync()
    {
        try
        {
            var subscription = await _jsRuntime.InvokeAsync<dynamic>("pushInterop.getSubscription");
            
            if (subscription == null)
            {
                return new NotificationSubscriptionResult
                {
                    Success = false,
                    Reason = "no-subscription",
                    Error = "No subscription found"
                };
            }

            return new NotificationSubscriptionResult
            {
                Success = true,
                Subscription = new NotificationSubscription
                {
                    Endpoint = subscription.Endpoint?.ToString() ?? "",
                    Keys = new NotificationKeys
                    {
                        P256dh = subscription.Keys?.P256dh?.ToString() ?? "",
                        Auth = subscription.Keys?.Auth?.ToString() ?? ""
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get subscription");
            return new NotificationSubscriptionResult
            {
                Success = false,
                Reason = "exception",
                Error = ex.Message
            };
        }
    }

    public async Task<bool> UnsubscribeAsync()
    {
        try
        {
            var endpoint = await _jsRuntime.InvokeAsync<string?>("pushInterop.unsubscribeUser");
            var success = !string.IsNullOrEmpty(endpoint);
            
            _logger.LogInformation("Unsubscribe result: {Success}, Endpoint: {Endpoint}", success, endpoint);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe from push notifications");
            return false;
        }
    }

    public async Task<bool> IsEnabledAsync()
    {
        try
        {
            var permission = await _jsRuntime.InvokeAsync<string>("pushInterop.getPermissionStatus");
            return permission == "granted";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check notification status");
            return false;
        }
    }

    public string GetPermissionStatus()
    {
        try
        {
            return _jsRuntime.InvokeAsync<string>("pushInterop.getPermissionStatus").GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get permission status");
            return "unknown";
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
