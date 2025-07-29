using Application.Contract.Order.Responses;

namespace Application.Contract.Order.Hub;

public interface IOrderRealtimeNotifier
{
    Task OrderCreated(OrderResponse order);
    Task OrderStatusChanged(OrderResponse order);
    Task OrderArchived(OrderResponse order);
}
