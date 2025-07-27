using Application.Contract.Order;
using Application.Contract.Order.Responses;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;

namespace WebApi.Services;

public class OrderRealtimeNotifier(IHubContext<OrderHub, IOrderClient> hub) : IOrderRealtimeNotifier
{
    public async Task OrderCreated(OrderResponse order)
    {
        await hub.Clients.Group($"shop:{order.ShopId}").OrderCreated(order);
        await hub.Clients.Group("superadmins").OrderCreated(order);
        await hub.Clients.Group($"customer:{order.Customer.Id}").OrderCreated(order);
    }

    public async Task OrderStatusChanged(OrderResponse order)
    {
        await hub.Clients.Group($"shop:{order.ShopId}").OrderStatusChanged(order);
        await hub.Clients.Group("superadmins").OrderStatusChanged(order);
        await hub.Clients.Group($"customer:{order.Customer.Id}").OrderStatusChanged(order);
    }

    public async Task OrderArchived(OrderResponse order)
    {
        await hub.Clients.Group($"shop:{order.ShopId}").OrderArchived(order);
        await hub.Clients.Group("superadmins").OrderArchived(order);
        await hub.Clients.Group($"customer:{order.Customer.Id}").OrderArchived(order);
    }
}
