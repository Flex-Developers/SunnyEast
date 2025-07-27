using Application.Contract.Order.Responses;
using Application.Contract.Enums;

namespace Client.Infrastructure.Realtime;

public interface IOrderRealtimeService : IAsyncDisposable
{
    event Action<OrderResponse>? OnOrderCreated;
    event Action<string, OrderStatus, DateTime?, DateTime?>? OnOrderStatusChanged;
    event Action<string>? OnOrderArchived;

    Task StartAsync();
    Task StopAsync();
}
