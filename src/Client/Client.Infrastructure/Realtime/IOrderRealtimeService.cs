using Application.Contract.Order.Responses;

namespace Client.Infrastructure.Realtime;

public interface IOrderRealtimeService
{
    event Action<OrderResponse>? OnOrderCreated;
    event Action<OrderResponse>? OnOrderStatusChanged;
    event Action<OrderResponse>? OnOrderArchived;

    Task StartAsync();
    Task StopAsync();
}
