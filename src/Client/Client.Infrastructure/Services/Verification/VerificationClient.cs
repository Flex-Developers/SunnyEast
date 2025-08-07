using Application.Contract.Verification.Commands;
using Application.Contract.Verification.Responses;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.Verification;

public sealed class VerificationClient(IHttpClientService http) : IVerificationClient
{
    public async Task<CheckAvailabilityResponse> CheckAvailabilityAsync(string? email, string? phone)
    {
        var res = await http.GetFromJsonAsync<CheckAvailabilityResponse>(
            $"/api/verification/check-availability?email={Uri.EscapeDataString(email ?? "")}&phone={Uri.EscapeDataString(phone ?? "")}");

        if (!res.Success || res.Response is null)
            throw new InvalidOperationException(http.ExceptionMessage ?? "Не удалось проверить доступность.");

        return res.Response;
    }

    public async Task<StartVerificationResponse> StartAsync(StartVerificationCommand req)
    {
        var res = await http.PostAsJsonAsync<StartVerificationResponse>("/api/verification/start", req);

        if (!res.Success || res.Response is null)
            throw new InvalidOperationException(http.ExceptionMessage ?? "Не удалось запустить верификацию.");

        return res.Response;
    }

    public async Task<ResendResponse> ResendAsync(string sessionId)
    {
        var res = await http.PostAsJsonAsync<ResendResponse>(
            "/api/verification/resend",
            new ResendCodeCommand { SessionId = sessionId });

        if (!res.Success || res.Response is null)
            throw new InvalidOperationException(http.ExceptionMessage ?? "Не удалось отправить код повторно.");

        return res.Response;
    }

    public async Task<VerifyResponse> VerifyAsync(string sessionId, string code)
    {
        var res = await http.PostAsJsonAsync<VerifyResponse>(
            "/api/verification/verify",
            new VerifyCodeCommand { SessionId = sessionId, Code = code });

        if (!res.Success || res.Response is null)
            throw new InvalidOperationException(http.ExceptionMessage ?? "Проверка кода не удалась.");

        return res.Response;
    }

    public async Task<GetSessionStateResponse> GetStateAsync(string sessionId)
    {
        var res = await http.GetFromJsonAsync<GetSessionStateResponse>(
            $"/api/verification/session/{Uri.EscapeDataString(sessionId)}");

        if (!res.Success || res.Response is null)
            throw new InvalidOperationException(http.ExceptionMessage ?? "Не удалось получить состояние сессии.");

        return res.Response;
    }
}
