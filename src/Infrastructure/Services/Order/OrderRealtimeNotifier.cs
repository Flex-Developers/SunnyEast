using Application.Common.Interfaces.Services;
using Application.Contract.Order.Hub;
using Application.Contract.Order.Responses;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services.Order;

public sealed class OrderRealtimeNotifier(
    IHubContext<OrderHub, IOrderClient> hub,
    IPushNotificationService pushNotificationService) : IOrderRealtimeNotifier
{
    public async Task OrderCreated(OrderResponse order)
    {
        await hub.Clients.Group(OrderGroupNames.Shop(order.ShopId)).OrderCreated(order);
        await hub.Clients.Group(OrderGroupNames.SuperAdminsGroup).OrderCreated(order);
        await hub.Clients.Group(OrderGroupNames.Customer(order.Customer.Id)).OrderCreated(order);
        await pushNotificationService.SendCreateOrderNotificationAsync(order);
    }

    public async Task OrderStatusChanged(OrderResponse order)
    {
        await hub.Clients.Group(OrderGroupNames.Shop(order.ShopId)).OrderStatusChanged(order);
        await hub.Clients.Group(OrderGroupNames.SuperAdminsGroup).OrderStatusChanged(order);
        await hub.Clients.Group(OrderGroupNames.Customer(order.Customer.Id)).OrderStatusChanged(order);
    }

    public async Task OrderArchived(OrderResponse order)
    {
        await hub.Clients.Group(OrderGroupNames.Shop(order.ShopId)).OrderArchived(order);
        await hub.Clients.Group(OrderGroupNames.SuperAdminsGroup).OrderArchived(order);
        await hub.Clients.Group(OrderGroupNames.Customer(order.Customer.Id)).OrderArchived(order);
    }
}