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

public class WebPushNotificationService : IPushNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WebPushNotificationService> _logger;
    private readonly WebPushClient _webPushClient;

    public WebPushNotificationService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<WebPushNotificationService> logger)
    {
        _context = context;
        _logger = logger;
        _webPushClient = new WebPushClient();

        // Set VAPID details from configuration
        var publicKey = configuration["WebPush:PublicKey"];
        var privateKey = configuration["WebPush:PrivateKey"];
        var subject = configuration["WebPush:Subject"] ?? "mailto:admin@yourapp.com";

        if (!string.IsNullOrEmpty(publicKey) && !string.IsNullOrEmpty(privateKey))
        {
            _webPushClient.SetVapidDetails(subject, publicKey, privateKey);
            _logger.LogInformation("WebPushNotificationService initialized successfully with VAPID details");
        }
        else
        {
            _logger.LogWarning("WebPushNotificationService initialized without VAPID details. Push notifications may not work properly");
        }
    }

    public async Task PushAsync(Guid userId, string actionType, string header, string body)
    {
        _logger.LogInformation("Starting push notification for user {UserId} with action type {ActionType}", userId, actionType);
        
        try
        {
            // Get all notification subscriptions for the user
            _logger.LogDebug("Retrieving notification subscriptions for user {UserId}", userId);
            var subscriptions = await _context.Set<NotificationSubscription>()
                .Where(s => s.UserId == userId)
                .ToListAsync();

            if (!subscriptions.Any())
            {
                _logger.LogWarning("No notification subscriptions found for user {UserId}", userId);
                return;
            }

            _logger.LogInformation("Found {SubscriptionCount} notification subscriptions for user {UserId}", subscriptions.Count, userId);

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
            _logger.LogDebug("Created notification payload for user {UserId}: {Payload}", userId, payloadJson);

            // Send notification to all user's subscriptions
            var tasks = subscriptions.Select(async subscription =>
            {
                try
                {
                    _logger.LogDebug("Sending push notification to endpoint {Endpoint} for user {UserId}", subscription.Endpoint, userId);
                    
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
                        "Failed to send push notification to user {UserId} endpoint {Endpoint}. Status: {StatusCode}, Message: {Message}",
                        userId, subscription.Endpoint, ex.StatusCode, ex.Message);

                    // If subscription is no longer valid, remove it
                    if (ex.StatusCode == System.Net.HttpStatusCode.Gone ||
                        ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogInformation("Removing invalid subscription for user {UserId} due to status code {StatusCode}", userId, ex.StatusCode);
                        _context.Set<NotificationSubscription>().Remove(subscription);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Successfully removed invalid subscription for user {UserId}", userId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error sending push notification to user {UserId} endpoint {Endpoint}", userId, subscription.Endpoint);
                }
            });

            await Task.WhenAll(tasks);
            _logger.LogInformation("Completed push notification sending for user {UserId} to {SubscriptionCount} subscriptions", userId, subscriptions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in PushAsync for user {UserId} with action type {ActionType}", userId, actionType);
            throw;
        }
    }

    public async Task SendCreateOrderNotificationAsync(OrderResponse order)
    {
        _logger.LogInformation("Starting order creation notification for order {OrderSlug} in shop {ShopId}", order.Slug, order.ShopId);
        
        try
        {
            _logger.LogDebug("Retrieving salesman staff members for shop {ShopId}", order.ShopId);
            var receivers = await _context.Staff
                .Where(f => f.StaffRole == StaffRole.Salesman && f.ShopId == order.ShopId)
                .ToListAsync();

            if (!receivers.Any())
            {
                _logger.LogWarning("No salesman staff members found for shop {ShopId} to notify about order {OrderSlug}", order.ShopId, order.Slug);
                return;
            }

            _logger.LogInformation("Found {ReceiverCount} salesman staff members to notify about order {OrderSlug}", receivers.Count, order.Slug);

            var notificationsSent = 0;
            var notificationsFailed = 0;

            foreach (var receiver in receivers)
            {
                try
                {
                    _logger.LogDebug("Processing notification for staff member {UserId} for order {OrderSlug}", receiver.UserId, order.Slug);
                    
                    var receiverSubscriptions =
                        await _context.NotificationSubscriptions.FirstOrDefaultAsync(f => f.UserId == receiver.UserId);
                    
                    if (receiverSubscriptions == null)
                    {
                        _logger.LogWarning("No notification subscription found for staff member {UserId}", receiver.UserId);
                        continue;
                    }

                    var payload = new
                    {
                        title = "Новый заказ",
                        body = $"Получен новый заказ #{order.OrderNumber}",
                        actionType = "new_order",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        data = new
                        {
                            orderSlug = order.Slug,
                            orderNumber = order.OrderNumber,
                            shopId = order.ShopId
                        }
                    };

                    await _webPushClient.SendNotificationAsync(
                        new PushSubscription(receiverSubscriptions.Endpoint, receiverSubscriptions.P256Dh, receiverSubscriptions.Auth), 
                        JsonSerializer.Serialize(payload)
                    );

                    notificationsSent++;
                    _logger.LogInformation("Successfully sent order notification to staff member {UserId} for order {OrderSlug}", receiver.UserId, order.Slug);
                }
                catch (WebPushException ex)
                {
                    notificationsFailed++;
                    _logger.LogError(ex, "Failed to send order notification to staff member {UserId} for order {OrderSlug}. Status: {StatusCode}", 
                        receiver.UserId, order.Slug, ex.StatusCode);

                    // Handle invalid subscriptions
                    if (ex.StatusCode == System.Net.HttpStatusCode.Gone ||
                        ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var subscription = await _context.NotificationSubscriptions.FirstOrDefaultAsync(f => f.UserId == receiver.UserId);
                        if (subscription != null)
                        {
                            _context.NotificationSubscriptions.Remove(subscription);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Removed invalid subscription for staff member {UserId}", receiver.UserId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    notificationsFailed++;
                    _logger.LogError(ex, "Unexpected error sending order notification to staff member {UserId} for order {OrderSlug}", receiver.UserId, order.Slug);
                }
            }

            _logger.LogInformation("Order notification process completed for order {OrderSlug}. Sent: {SentCount}, Failed: {FailedCount}", 
                order.Slug, notificationsSent, notificationsFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in SendCreateOrderNotificationAsync for order {OrderSlug}", order.Slug);
            throw;
        }
    }
}