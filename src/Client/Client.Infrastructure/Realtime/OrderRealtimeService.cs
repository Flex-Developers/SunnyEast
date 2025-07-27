using Application.Contract.Order.Responses;
using Application.Contract.User.Responses;
using Blazored.LocalStorage;
using Client.Infrastructure.Consts;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace Client.Infrastructure.Realtime;

public class OrderRealtimeService(ILocalStorageService storage, IConfiguration config) : IOrderRealtimeService, IAsyncDisposable
{
    private HubConnection? _connection;

    public event Action<OrderResponse>? OnOrderCreated;
    public event Action<OrderResponse>? OnOrderStatusChanged;
    public event Action<OrderResponse>? OnOrderArchived;

    public async Task StartAsync()
    {
        if (_connection is not null)
            return;

        var baseUrl = config[Config.ApiBaseUrl];
        var url = new Uri(new Uri(baseUrl!), "/hubs/orders");

        _connection = new HubConnectionBuilder()
            .WithUrl(url, options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    var token = await storage.GetItemAsync<JwtTokenResponse>("authToken");
                    return token?.AccessToken;
                };
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<OrderResponse>("OrderCreated", o => OnOrderCreated?.Invoke(o));
        _connection.On<OrderResponse>("OrderStatusChanged", o => OnOrderStatusChanged?.Invoke(o));
        _connection.On<OrderResponse>("OrderArchived", o => OnOrderArchived?.Invoke(o));

        try
        {
            await _connection.StartAsync();
        }
        catch
        {
            // ignore
        }
    }

    public async Task StopAsync()
    {
        if (_connection is not null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public async ValueTask DisposeAsync() => await StopAsync();
}
