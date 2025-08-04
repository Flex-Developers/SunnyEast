using Application.Contract.User.Responses;

namespace Client.Infrastructure.Services.Verification;

public enum OtpChannel { phone, email }

public sealed record StartVerificationRequest(
    string Purpose,
    string? UserName = null, // для register
    string? Email   = null,  // опционально для reset
    string? Phone   = null   // опционально для reset
);

public sealed record StartVerificationResponse(
    string SessionId,
    OtpChannel[] Available,
    OtpChannel Selected,
    string? MaskedPhone,
    string? MaskedEmail,
    int CodeLength,
    int CooldownSeconds,
    int TtlSeconds,
    int AttemptsLeft
);

public sealed record SwitchChannelRequest(string SessionId, OtpChannel Channel);
public sealed record SwitchChannelResponse(OtpChannel Selected, int CooldownSeconds, int CodeLength);

public sealed record ResendRequest(string SessionId);
public sealed record ResendResponse(int CooldownSeconds);

// Для login-verify сервер вернёт токен
public sealed record VerifyResponse(bool Success, string? ReturnUrl, JwtTokenResponse? Token);

// НОВОЕ: получить состояние по sid (подцепление к уже созданной сессии)
public sealed record GetSessionStateResponse(
    string SessionId,
    OtpChannel[] Available,
    OtpChannel Selected,
    string? MaskedPhone,
    string? MaskedEmail,
    int CodeLength,
    int CooldownSeconds,
    int TtlSeconds,
    int AttemptsLeft
);

public interface IVerificationService
{
    Task<StartVerificationResponse> StartAsync(StartVerificationRequest req);
    Task<SwitchChannelResponse> SwitchChannelAsync(SwitchChannelRequest req);
    Task<ResendResponse> ResendAsync(ResendRequest req);
    Task<VerifyResponse> VerifyAsync(VerifyRequest req);
    Task<GetSessionStateResponse> GetStateAsync(string sessionId); // НОВОЕ
    
    // New
    Task<(int cooldown, int ttl)> RequestSmsAsync(string phone, string purpose);
    Task<ConfirmRegResult> ConfirmRegistrationAsync(string phone, string code, string email, string password, string? fullName = null);
    Task<bool> ConfirmResetAsync(string phone, string code, string newPassword);
}

public sealed record VerifyRequest(string SessionId, string Code);
public sealed record ConfirmRegResult(bool Success, JwtTokenResponse? Token);