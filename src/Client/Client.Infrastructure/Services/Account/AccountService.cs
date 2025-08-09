using System.Net;
using System.Net.Http.Json;
using Application.Contract.Account.Commands;
using Application.Contract.Account.Responses;
using Application.Contract.User.Responses;
using Application.Contract.Verification.Responses;
using Client.Infrastructure.Auth;
using Client.Infrastructure.Services.HttpClient;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Client.Infrastructure.Services.Account;

public sealed class AccountService(IHttpClientService http, CustomAuthStateProvider auth, ISnackbar snackbar)
    : IAccountService
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

    public async Task<bool> ResetPasswordAsync(string sessionId, string newPassword, string confirm)
    {
        var cmd = new ResetPasswordCommand
        {
            SessionId = sessionId,
            NewPassword = newPassword,
            ConfirmPassword = confirm
        };

        var resp = await http.PostAsJsonAsync("/api/account/reset-password", cmd);
        return resp.Success;
    }

    public async Task<StartVerificationResponse> StartLinkEmailAsync(string newEmail)
    {
        var res = await http.PostAsJsonAsync<StartVerificationResponse>(
            "/api/account/link-contact",
            new { channel = "email", value = newEmail });
        if (!res.Success || res.Response is null)
            throw new InvalidOperationException(http.ExceptionMessage ?? "Не удалось начать верификацию.");

        return res.Response;
    }
    
    public async Task<StartVerificationResponse> StartLinkPhoneAsync(string newPhone)
    {
        var res = await http.PostAsJsonAsync<StartVerificationResponse>(
            "/api/account/link-contact",
            new { channel = "phone", value = newPhone });
        if (!res.Success || res.Response is null)
            throw new InvalidOperationException(http.ExceptionMessage ?? "Не удалось начать верификацию.");

        return res.Response;
    }
    
    public async Task<bool> ConfirmLinkAsync(string sessionId)
    {
        var res = await http.PostAsJsonAsync("/api/account/confirm-link",
            new { sessionId });
        return res.Success;
    }
}