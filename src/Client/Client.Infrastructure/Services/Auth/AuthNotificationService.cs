using Client.Infrastructure.Services.Notifications;
using Microsoft.Extensions.Logging;

namespace Client.Infrastructure.Services.Auth;

public interface IAuthNotificationService
{
    Task HandlePostLoginNotificationSetupAsync();
    Task HandleLogoutNotificationCleanupAsync();
}

public class AuthNotificationService : IAuthNotificationService
{
    private readonly INotificationManager _notificationManager;
    private readonly ILogger<AuthNotificationService> _logger;

    public AuthNotificationService(
        INotificationManager notificationManager,
        ILogger<AuthNotificationService> logger)
    {
        _notificationManager = notificationManager;
        _logger = logger;
    }

    public async Task HandlePostLoginNotificationSetupAsync()
    {
        try
        {
            _logger.LogInformation("Setting up notifications after login...");
            
            // Initialize the notification system
            await _notificationManager.InitializeAsync();
            
            // Check if notifications are already enabled
            var isEnabled = await _notificationManager.IsEnabledAsync();
            var permissionStatus = _notificationManager.GetPermissionStatus();
            
            _logger.LogInformation("Notification status after login - Enabled: {IsEnabled}, Permission: {Permission}", 
                isEnabled, permissionStatus);
            
            // If notifications are already granted but not subscribed, enable them
            if (permissionStatus == "granted" && !isEnabled)
            {
                _logger.LogInformation("Permission granted but not subscribed, enabling notifications...");
                await _notificationManager.EnableNotificationsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup notifications after login");
        }
    }

    public async Task HandleLogoutNotificationCleanupAsync()
    {
        try
        {
            _logger.LogInformation("Cleaning up notifications after logout...");
            
            // Optionally disable notifications on logout
            // You might want to keep them enabled for the user
            // await _notificationManager.DisableNotificationsAsync();
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup notifications after logout");
        }
    }
}
