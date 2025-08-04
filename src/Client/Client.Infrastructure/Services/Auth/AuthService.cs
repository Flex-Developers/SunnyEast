using Application.Contract.User.Commands;
using Application.Contract.User.Responses;
using Client.Infrastructure.Auth;
using Client.Infrastructure.Realtime;
using Client.Infrastructure.Services.HttpClient;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Infrastructure.Services.Auth;

public class AuthService(
    IHttpClientService httpClient,
    NavigationManager navigationManager,
    CustomAuthStateProvider authStateProvider,
    ISnackbar snackbar,
    IOrderRealtimeService orderRealtime) : IAuthService
{
    public async Task<bool> LoginAsync(LoginUserCommand command, string? returnUrl = null)
    {
        var loginResponse = await httpClient.PostAsJsonAsync<JwtTokenResponse>("/api/user/login", command);
        if (loginResponse.Success)
        {
            await authStateProvider.MarkUserAsAuthenticated(loginResponse.Response!);
            
            // теперь можно безопасно стартовать SignalR (токен доступен)
            try { await orderRealtime.StartAsync(); } catch { /* тихо игнорируем, автопереподключение позже */ }
            
            navigationManager.NavigateTo(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
        }

        return loginResponse.Success;
    }

    public async Task LogoutAsync(bool navigateToHome = false)
    {
        // сначала закрываем хаб, чтобы не было автопереподключений без токена
        try { await orderRealtime.StopAsync(); } catch { /* ignore */ }
        
        await authStateProvider.MarkUserAsLoggedOut();
        
        if (navigateToHome)
            navigationManager.NavigateTo("/");
        
        snackbar.Add("Вы вышли из аккаунта.", Severity.Success);
    }
}