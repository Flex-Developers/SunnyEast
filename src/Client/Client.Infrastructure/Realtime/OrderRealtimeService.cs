using Application.Contract.Order.Responses;
using Application.Contract.User.Responses;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Client.Infrastructure.Realtime;

public class OrderRealtimeService(ILocalStorageService localStorage, NavigationManager navigation) : IOrderRealtimeService, IAsyncDisposable
{
    private HubConnection? _connection;

    public event Action<OrderResponse>? OnOrderCreated;
    public event Action<OrderResponse>? OnOrderStatusChanged;
    public event Action<OrderResponse>? OnOrderArchived;

    public async Task StartAsync()
    {
        if (_connection is not null)
            return;

        var hubUrl = new Uri(new Uri(navigation.BaseUri), "/hubs/orders");
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    var token = await localStorage.GetItemAsync<JwtTokenResponse>("authToken");
                    return token?.AccessToken;
                };
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<OrderResponse>("OrderCreated", order => OnOrderCreated?.Invoke(order));
        _connection.On<OrderResponse>("OrderStatusChanged", order => OnOrderStatusChanged?.Invoke(order));
        _connection.On<OrderResponse>("OrderArchived", order => OnOrderArchived?.Invoke(order));

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task StopAsync()
    {
        if (_connection is null)
            return;

        await _connection.StopAsync();
        await _connection.DisposeAsync();
        _connection = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
