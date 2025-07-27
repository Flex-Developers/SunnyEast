using Application.Contract.Order;
using Application.Contract.Order.Responses;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;

namespace WebApi.Services;

public sealed class OrderRealtimeNotifier(IHubContext<OrderHub, IOrderClient> hub)
    : IOrderRealtimeNotifier
{
    public async Task OrderCreated(OrderResponse order)
    {
        await hub.Clients.Group(OrderHub.ShopGroup(order.ShopId)).OrderCreated(order);
        await hub.Clients.Group(OrderHub.SuperAdminsGroup).OrderCreated(order);
        await hub.Clients.Group(OrderHub.CustomerGroup(order.Customer.Id)).OrderCreated(order);
    }

    public async Task OrderStatusChanged(OrderResponse order)
    {
        await hub.Clients.Group(OrderHub.ShopGroup(order.ShopId)).OrderStatusChanged(order);
        await hub.Clients.Group(OrderHub.SuperAdminsGroup).OrderStatusChanged(order);
        await hub.Clients.Group(OrderHub.CustomerGroup(order.Customer.Id)).OrderStatusChanged(order);
    }

    public async Task OrderArchived(OrderResponse order)
    {
        await hub.Clients.Group(OrderHub.ShopGroup(order.ShopId)).OrderArchived(order);
        await hub.Clients.Group(OrderHub.SuperAdminsGroup).OrderArchived(order);
        await hub.Clients.Group(OrderHub.CustomerGroup(order.Customer.Id)).OrderArchived(order);
    }
}