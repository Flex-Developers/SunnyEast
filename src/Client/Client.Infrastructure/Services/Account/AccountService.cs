using System.Net;
using Application.Contract.Account.Commands;
using Application.Contract.Account.Responses;
using Application.Contract.User.Responses;
using Client.Infrastructure.Auth;
using Client.Infrastructure.Services.HttpClient;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Client.Infrastructure.Services.Account;

public sealed class AccountService(
    IHttpClientService http,
    CustomAuthStateProvider auth,
    ISnackbar snackbar) : IAccountService
{
    public async Task<MyAccountResponse?> GetAsync()
    {
        var res = await http.GetFromJsonAsync<MyAccountResponse>("/api/account/me");
        
        if (res.Success) 
            return res.Response;
        
        if (res.StatusCode == HttpStatusCode.Unauthorized) 
            snackbar.Add("Требуется вход в систему.", Severity.Error);
        
        return null;
    }

    public async Task<bool> UpdateProfileAsync(UpdateProfileCommand request)
    {
        var res = await http.PutAsJsonAsync("/api/account/profile", request);
        return res.Success;
    }

    public async Task<bool> ChangeEmailAsync(ChangeEmailCommand request)
    {
        var res = await http.PutAsJsonAsync("/api/account/email", request);
        return res.Success;
    }

    public async Task<bool> ChangePhoneAsync(ChangePhoneCommand request)
    {
        var res = await http.PutAsJsonAsync("/api/account/phone", request);
        return res.Success;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordCommand request)
    {
        var res = await http.PutAsJsonAsync("/api/account/password", request);
        return res.Success;
    }

    public async Task<bool> LogoutAllAsync()
    {
        var res = await http.PostAsJsonAsync("/api/account/logout-all", new { });
        return res.Success;
    }
    
    public async Task<bool> DeleteAccountAsync()
    {
        var res = await http.DeleteAsync("/api/account");
        return res.Success;
    }


    public async Task<JwtTokenResponse?> RefreshTokenAsync()
    {
        var res = await http.PostAsJsonAsync<JwtTokenResponse>("/api/account/refresh-token", new { });
        if (!res.Success) 
            return null;
        
        await auth.MarkUserAsAuthenticated(res.Response!);
        return res.Response;
    }
}