using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace Client.Infrastructure.Auth.Jwt;

public class JwtAuthenticationHeaderHandler(
    IAccessTokenProviderAccessor tokenProviderAccessor,
    NavigationManager navigation)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // skip token endpoints
        if (request.RequestUri?.AbsolutePath.Contains("/tokens") is not true)
        {
            if ((await tokenProviderAccessor.TokenProvider.RequestAccessToken()).TryGetToken(out var accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
            }
            else
            {
                navigation.NavigateTo("/login");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}