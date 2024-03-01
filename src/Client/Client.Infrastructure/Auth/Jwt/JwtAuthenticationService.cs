using System.Security.Claims;

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Client.Infrastructure.Auth.Jwt;

public class JwtAuthenticationService(ILocalStorageService localStorage, NavigationManager navigation)
    : AuthenticationStateProvider, IAuthenticationService, IAccessTokenProvider
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public AuthProvider ProviderType => AuthProvider.Jwt;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return new AuthenticationState(new ClaimsPrincipal(new List<ClaimsIdentity>()));
    }

    public void NavigateToExternalLogin(string returnUrl) =>
        throw new NotImplementedException();
    

    public Task LogoutAsync()
    {
        throw new NotImplementedException();
    }

    public Task ReLoginAsync(string returnUrl)
    {
        throw new NotImplementedException();
    }

    public ValueTask<AccessTokenResult> RequestAccessToken()
    {
        throw new NotImplementedException();
    }

    public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
    {
        throw new NotImplementedException();
    }
}