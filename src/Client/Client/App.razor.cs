using System.Runtime.InteropServices;
using Application.Contract.User.Responses;
using Blazored.LocalStorage;
using Client.Infrastructure.Realtime;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client;

public partial class App : IAsyncDisposable
{
    [Inject] private IOrderRealtimeService RealtimeService { get; set; } = default!;
    [Inject] private ILocalStorageService Storage { get; set; } = default!;
    [Inject] private AuthenticationStateProvider Auth { get; set; } = default!;

    private bool _started;

    protected override void OnInitialized()
    {
        // следим за логином/выходом и перезапускаем хаб
        Auth.AuthenticationStateChanged += OnAuthChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_started)
        {
            _started = true;
            
            var token = await Storage.GetItemAsync<JwtTokenResponse>("authToken");
            
            if (!string.IsNullOrWhiteSpace(token?.AccessToken))
                await TryStartAsync();
        }
    }

    private async void OnAuthChanged(Task<AuthenticationState> task)
    {
        var state = await task;
        if (state.User.Identity?.IsAuthenticated == true)
            await TryStartAsync();
        else
            await RealtimeService.StopAsync();
    }

    private async Task TryStartAsync()
    {
        try
        {
            await RealtimeService.StartAsync(); // StartAsync сам проверит состояние
        }
        catch (Exception ex)
        {
            // не ломать UI, но логировать для отладки
            Console.WriteLine($"RealtimeService failed to start: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Auth is not null)
            Auth.AuthenticationStateChanged -= OnAuthChanged;

        await RealtimeService.StopAsync();
    }
}