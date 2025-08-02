using Application.Contract.Order.Responses;

namespace Application.Common.Interfaces.Services;

public interface IPushNotificationService
{
    Task SendCreateOrderNotificationAsync(OrderResponse order);
    Task SendOrderStatusUpdateNotificationAsync(OrderResponse order);
    Task SendOrderArchivedNotificationAsync(OrderResponse order);
}