using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;

namespace Client.Infrastructure.Realtime;

public sealed class OrderRealtimeService : IOrderRealtimeService
{
    private readonly NavigationManager _nav;
    private readonly ILocalStorageService _localStorage;
    private HubConnection? _hub;

    public event Action<Application.Contract.Order.Responses.OrderResponse>? OnOrderCreated;
    public event Action<string, Application.Contract.Enums.OrderStatus, DateTime?, DateTime?>? OnOrderStatusChanged;
    public event Action<string>? OnOrderArchived;

    public OrderRealtimeService(NavigationManager nav, ILocalStorageService localStorage)
    {
        _nav = nav;
        _localStorage = localStorage;
    }

    public async Task StartAsync()
    {
        if (_hub is { State: HubConnectionState.Connected }) return;

        var baseUri = _nav.BaseUri.TrimEnd('/');
        _hub = new HubConnectionBuilder()
            .WithUrl($"{baseUri}/hubs/orders", options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    var tk = await _localStorage.GetItemAsync<Application.Contract.User.Responses.JwtTokenResponse>("authToken");
                    return tk?.AccessToken;
                };
            })
            .WithAutomaticReconnect()
            .Build();

        _hub.On<Application.Contract.Order.Responses.OrderResponse>("OrderCreated", o => OnOrderCreated?.Invoke(o));
        _hub.On<string, Application.Contract.Enums.OrderStatus, DateTime?, DateTime?>("OrderStatusChanged",
            (slug, status, closedAt, canceledAt) => OnOrderStatusChanged?.Invoke(slug, status, closedAt, canceledAt));
        _hub.On<string>("OrderArchived", slug => OnOrderArchived?.Invoke(slug));

        await _hub.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_hub is { State: HubConnectionState.Connected })
            await _hub.StopAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null) await _hub.DisposeAsync();
    }
}
