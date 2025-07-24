using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Contract.User.Responses;
using Blazored.LocalStorage;
using Client.Infrastructure.Services.Cart;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client.Infrastructure.Auth;

public class CustomAuthStateProvider(ILocalStorageService localStorageService, ICartService cartService) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await localStorageService.GetItemAsync<JwtTokenResponse>("authToken");
            if (token != null && !string.IsNullOrEmpty(token.AccessToken))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token.AccessToken);

                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    await localStorageService.RemoveItemAsync("authToken");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var claims = jwtToken.Claims;
                var identity = new ClaimsIdentity(claims, "jwt");
                var principal = new ClaimsPrincipal(identity);
                return new AuthenticationState(principal);
            }
        }
        catch (Exception)
        {
            await localStorageService.RemoveItemAsync("authToken");
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public async Task MarkUserAsAuthenticated(JwtTokenResponse token)
    {
        await localStorageService.SetItemAsync("authToken", token);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token.AccessToken);
        var claims = jwtToken.Claims;
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await localStorageService.RemoveItemAsync("authToken");
        await cartService.ClearAsync();
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }
}