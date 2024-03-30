using Application.Contract.User.Commands;
using Application.Contract.User.Responses;
using Blazored.LocalStorage;
using Client.Infrastructure.Services.HttpClient;
using Application.Common.Interfaces.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Client.Infrastructure.Services.Auth;

public class AuthService(
    IHttpClientService httpClient,
    ILocalStorageService localStorageService,
    IApplicationDbContext context,
    NavigationManager navigationManager) : IAuthService
{
    public async Task<bool> LoginAsync(LoginUserCommand command)
    {
        var loginResponse = await httpClient.PostAsJsonAsync<JwtTokenResponse>("/api/user/login", command);
        if (loginResponse.Success)
        {
            await localStorageService.SetItemAsync("authToken", loginResponse.Response);
            navigationManager.NavigateTo("/");
        }

        return loginResponse.Success;
    }

    public async Task LogoutAsync()
    {
        await localStorageService.RemoveItemAsync("authToken");
    }

    public async Task<bool> IsUsernameExistsAsync(string username)
    {
        return await context.Users.AnyAsync(u => u.UserName == username);
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await context.Users.AnyAsync(u => u.Email == email);
    }
}