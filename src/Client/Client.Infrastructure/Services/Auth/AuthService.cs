﻿using Application.Contract.User.Commands;
using Application.Contract.User.Responses;
using Client.Infrastructure.Auth;
using Client.Infrastructure.Services.HttpClient;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Infrastructure.Services.Auth;

public class AuthService(
    IHttpClientService httpClient,
    NavigationManager navigationManager,
    CustomAuthStateProvider authStateProvider,
    ISnackbar snackbar) : IAuthService
{
    public async Task<bool> LoginAsync(LoginUserCommand command, string? returnUrl = null)
    {
        var loginResponse = await httpClient.PostAsJsonAsync<JwtTokenResponse>("/api/user/login", command);
        if (loginResponse.Success)
        {
            await authStateProvider.MarkUserAsAuthenticated(loginResponse.Response!);
            navigationManager.NavigateTo(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
        }

        return loginResponse.Success;
    }

    public async Task LogoutAsync()
    {
        await authStateProvider.MarkUserAsLoggedOut();
        snackbar.Add("Вы вышли из аккаунта.", Severity.Success);
    }
}