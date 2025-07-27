namespace Application.Contract.Realtime;

using Application.Contract.Order.Responses;

public interface IOrderRealtimeNotifier
{
    Task OrderCreated(OrderResponse order);
    Task OrderStatusChanged(OrderResponse order);
    Task OrderArchived(OrderResponse order);
}
