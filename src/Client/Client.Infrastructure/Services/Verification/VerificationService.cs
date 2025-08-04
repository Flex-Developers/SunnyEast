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
}