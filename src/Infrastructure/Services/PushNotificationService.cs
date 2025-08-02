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
                try
                {
                    await _webPushClient.SendNotificationAsync(new PushSubscription(receiverSubscriptions.Endpoint,
                            receiverSubscriptions.P256Dh, receiverSubscriptions.Auth), JsonSerializer.Serialize(new
                        {
                            title = "Новый заказ",
                            body = ""
                        })
                    );
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
            }
        }
    }

    public async Task SendOrderStatusUpdateNotificationAsync(OrderResponse order)
    {
        var customerSubscriptions = await _context.NotificationSubscriptions
            .Where(f => f.UserId == order.Customer.Id).ToListAsync();

        foreach (var subscription in customerSubscriptions)
        {
            try
            {
                await _webPushClient.SendNotificationAsync(new PushSubscription(subscription.Endpoint,
                        subscription.P256Dh, subscription.Auth), JsonSerializer.Serialize(new
                    {
                        title = "Статус заказа изменен",
                        body = "Ваш заказ обновлен"
                    })
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
    }

    public async Task SendOrderArchivedNotificationAsync(OrderResponse order)
    {
        var customerSubscriptions = await _context.NotificationSubscriptions
            .FirstOrDefaultAsync(f => f.UserId == order.Customer.Id);

        if (customerSubscriptions != null)
        {
            try
            {
                await _webPushClient.SendNotificationAsync(new PushSubscription(customerSubscriptions.Endpoint,
                        customerSubscriptions.P256Dh, customerSubscriptions.Auth), JsonSerializer.Serialize(new
                    {
                        title = "Заказ архивирован",
                        body = "Ваш заказ завершен"
                    })
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
    }
}