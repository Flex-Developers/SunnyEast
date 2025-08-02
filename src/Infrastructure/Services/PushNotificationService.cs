using Application.Common.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebPush;
using System.Text.Json;
using Application.Contract.Order.Responses;
using Domain.Enums;
using Infrastructure.Persistence.Contexts;

namespace Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly WebPushClient _webPushClient;

    public PushNotificationService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<PushNotificationService> logger)
    {
        _context = context;
        _logger = logger;
        _webPushClient = new WebPushClient();

        // Set VAPID details from configuration
        var publicKey = configuration["VapidKeys:PublicKey"];
        var privateKey = configuration["VapidKeys:PrivateKey"];
        var subject = configuration["VapidKeys:Subject"] ?? "mailto:admin@yourapp.com";

        if (!string.IsNullOrEmpty(publicKey) && !string.IsNullOrEmpty(privateKey))
        {
            _webPushClient.SetVapidDetails(subject, publicKey, privateKey);
        }
    }

    public async Task PushAsync(Guid userId, string actionType, string header, string body)
    {
        try
        {
            // Get all notification subscriptions for the user
            var subscriptions = await _context.Set<NotificationSubscription>()
                .Where(s => s.UserId == userId)
                .ToListAsync();

            if (!subscriptions.Any())
            {
                _logger.LogWarning("No notification subscriptions found for user {UserId}", userId);
                return;
            }

            // Create the notification payload
            var payload = new
            {
                title = header,
                body = body,
                actionType = actionType,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                icon = "/icon-192x192.png", // Default icon, can be made configurable
                badge = "/badge-72x72.png", // Default badge, can be made configurable
                data = new
                {
                    actionType = actionType,
                    userId = userId
                }
            };

            var payloadJson = JsonSerializer.Serialize(payload);

            // Send notification to all user's subscriptions
            var tasks = subscriptions.Select(async subscription =>
            {
                try
                {
                    var pushSubscription = new PushSubscription(
                        subscription.Endpoint,
                        subscription.P256Dh,
                        subscription.Auth);

                    await _webPushClient.SendNotificationAsync(pushSubscription, payloadJson);
                    _logger.LogInformation("Push notification sent successfully to user {UserId} endpoint {Endpoint}",
                        userId, subscription.Endpoint);
                }
                catch (WebPushException ex)
                {
                    _logger.LogError(ex,
                        "Failed to send push notification to user {UserId} endpoint {Endpoint}. Status: {StatusCode}",
                        userId, subscription.Endpoint, ex.StatusCode);

                    // If subscription is no longer valid, remove it
                    if (ex.StatusCode == System.Net.HttpStatusCode.Gone ||
                        ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _context.Set<NotificationSubscription>().Remove(subscription);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Removed invalid subscription for user {UserId}", userId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error sending push notification to user {UserId}", userId);
                }
            });

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PushAsync for user {UserId}", userId);
            throw;
        }
    }

    public async Task SendCreateOrderNotificationAsync(OrderResponse order)
    {
        var receivers = await _context.Staff
            .Where(f => f.StaffRole == StaffRole.Salesman && f.ShopId == order.ShopId)
            .ToListAsync();

        foreach (var receiver in receivers)
        {
            var receiverSubscriptions =
                await _context.NotificationSubscriptions.FirstOrDefaultAsync(f => f.UserId == receiver.UserId);
            if (receiverSubscriptions != null)
            {
                await _webPushClient.SendNotificationAsync(new PushSubscription(receiverSubscriptions.Endpoint,
                        receiverSubscriptions.P256Dh, receiverSubscriptions.Auth), JsonSerializer.Serialize(new
                    {
                        title = "Новый заказ",
                        body = ""
                    })
                );
            }
        }
    }
}