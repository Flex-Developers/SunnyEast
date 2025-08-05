using Application.Contract.Verification.Commands;
using Application.Contract.Verification.Responses;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.Verification;

public sealed class VerificationClient(IHttpClientService http) : IVerificationClient
{
    public async Task<CheckAvailabilityResponse> CheckAvailabilityAsync(string? email, string? phone)
        => (await http.GetFromJsonAsync<CheckAvailabilityResponse>($"/api/verification/check-availability?email={Uri.EscapeDataString(email ?? "")}&phone={Uri.EscapeDataString(phone ?? "")}")).Response!;

    public async Task<StartVerificationResponse> StartAsync(StartVerificationCommand req)
        => (await http.PostAsJsonAsync<StartVerificationResponse>("/api/verification/start", req)).Response!;

    public async Task<ResendResponse> ResendAsync(string sessionId)
        => (await http.PostAsJsonAsync<ResendResponse>("/api/verification/resend", new ResendCodeCommand { SessionId = sessionId })).Response!;

    public async Task<VerifyResponse> VerifyAsync(string sessionId, string code)
        => (await http.PostAsJsonAsync<VerifyResponse>("/api/verification/verify", new VerifyCodeCommand { SessionId = sessionId, Code = code })).Response!;

    public async Task<GetSessionStateResponse> GetStateAsync(string sessionId)
        => (await http.GetFromJsonAsync<GetSessionStateResponse>($"/api/verification/session/{Uri.EscapeDataString(sessionId)}")).Response!;
}