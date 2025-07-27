using Client.Infrastructure.Realtime;
using Microsoft.AspNetCore.Components;

namespace Client;

public partial class App : IAsyncDisposable
{
    [Inject] private IOrderRealtimeService RealtimeService { get; set; } = default!;
    private bool _started;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_started)
        {
            _started = true;
            await RealtimeService.StartAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await RealtimeService.StopAsync();
    }
}
