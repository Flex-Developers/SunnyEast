using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Contract.User.Responses;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client.Infrastructure.Auth;

public class CustomAuthStateProvider(ILocalStorageService localStorageService) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorageService.GetItemAsync<JwtTokenResponse>("authToken");
        if (token != null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = tokenHandler.ReadJwtToken(token.AccessToken).Claims;
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(principal);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
            return authState;
        }

        return new AuthenticationState(new ClaimsPrincipal());
    }
}