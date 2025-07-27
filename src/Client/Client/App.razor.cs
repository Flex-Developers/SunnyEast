using Client.Infrastructure.Realtime;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client;

public partial class App : IAsyncDisposable
{
    [Inject] private IOrderRealtimeService RealtimeService { get; set; } = default!;
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
        await RealtimeService.StartAsync(); // StartAsync сам проверит состояние
    }

    public async ValueTask DisposeAsync()
    {
        if (Auth is not null)
            Auth.AuthenticationStateChanged -= OnAuthChanged;

        await RealtimeService.StopAsync();
    }
}