using Application.Contract.Order.Responses;

namespace Application.Contract.Order.Hub;

/// <summary>
/// Методы, которые вызываются сервером на стороне клиента.
/// </summary>
public interface IOrderClient
{
    Task OrderCreated(OrderResponse order);
    Task OrderStatusChanged(OrderResponse order);
    Task OrderArchived(OrderResponse order);
}