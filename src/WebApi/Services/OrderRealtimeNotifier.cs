using Application.Contract.Realtime;
using Application.Contract.Order.Responses;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;

namespace WebApi.Services;

public class OrderRealtimeNotifier(IHubContext<OrderHub, IOrderClient> hub) : IOrderRealtimeNotifier
{
    public async Task OrderCreated(OrderResponse order)
    {
        var groups = new[] { $"shop:{order.ShopSlug}", "superadmins", $"customer:{order.Customer.Id}" };
        await hub.Clients.Groups(groups).OrderCreated(order);
    }

    public async Task OrderStatusChanged(OrderResponse order)
    {
        var groups = new[] { $"shop:{order.ShopSlug}", "superadmins", $"customer:{order.Customer.Id}" };
        await hub.Clients.Groups(groups).OrderStatusChanged(order);
    }

    public async Task OrderArchived(OrderResponse order)
    {
        var groups = new[] { $"shop:{order.ShopSlug}", "superadmins", $"customer:{order.Customer.Id}" };
        await hub.Clients.Groups(groups).OrderArchived(order);
    }
}
