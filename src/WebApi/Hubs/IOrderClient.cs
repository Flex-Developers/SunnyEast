using Application.Contract.Enums;
using Application.Contract.Order.Responses;

namespace WebApi.Hubs;

public interface IOrderClient
{
    Task OrderCreated(OrderResponse order);
    Task OrderStatusChanged(string slug, OrderStatus newStatus, DateTime? closedAt, DateTime? canceledAt);
    Task OrderArchived(string slug);
}
