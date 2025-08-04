using Application.Contract.User.Responses;
using Application.Contract.Verification.Commands;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.Verification;

public sealed class VerificationService(IHttpClientService http) : IVerificationService
{
    public async Task<StartVerificationResponse> StartAsync(StartVerificationRequest req)
        => (await http.PostAsJsonAsync<StartVerificationResponse>("/api/verification/start", req)).Response!;

    public async Task<SwitchChannelResponse> SwitchChannelAsync(SwitchChannelRequest req)
        => (await http.PostAsJsonAsync<SwitchChannelResponse>("/api/verification/switch-channel", req)).Response!;

    public async Task<ResendResponse> ResendAsync(ResendRequest req)
        => (await http.PostAsJsonAsync<ResendResponse>("/api/verification/resend", req)).Response!;

    public async Task<VerifyResponse> VerifyAsync(VerifyRequest req)
        => (await http.PostAsJsonAsync<VerifyResponse>("/api/verification/verify", req)).Response!;

    public async Task<GetSessionStateResponse> GetStateAsync(string sessionId)
        => (await http.GetFromJsonAsync<GetSessionStateResponse>($"/api/verification/session/{Uri.EscapeDataString(sessionId)}")).Response!;
    
    // New
    public async Task<(int cooldown, int ttl)> RequestSmsAsync(string phone, string purpose)
    {
        // было: dynamic  →  станет: RequestSmsResult
        var res = await http.PostAsJsonAsync<RequestSmsResult>(
            "/api/verify/request-sms",
            new { phone, purpose });

        if (!res.Success)
            throw new InvalidOperationException("Ошибка запроса SMS");

        // теперь Response типизирован → всё читается без JsonElement
        return (res.Response!.CooldownSeconds, res.Response!.TtlSeconds);
    }
    
    public async Task<ConfirmRegResult> ConfirmRegistrationAsync(string phone, string code, string email, string password, string? fullName = null)
    {
        var res = await http.PostAsJsonAsync<JwtTokenResponse>(
            "/api/verify/confirm-registration",
            new { phone, code, email, password, fullName });

        return new ConfirmRegResult(res.Success, res.Response);
    }

    public async Task<bool> ConfirmResetAsync(string phone, string code, string newPassword)
    {
        var res = await http.PostAsJsonAsync<object>("/api/verify/confirm-reset",
            new { phone, code, newPassword });
        return res.Success;
    }
    
    
}