using Application.Contract.Verification.Enums;

namespace Application.Common.Interfaces.Services;

public interface IVerificationSessionStore
{
    Task SaveAsync(VerificationSession session, CancellationToken ct);
    Task<VerificationSession?> GetAsync(string sessionId, CancellationToken ct);
    Task UpdateAsync(VerificationSession session, CancellationToken ct);
    Task RemoveAsync(string sessionId, CancellationToken ct);
}

public sealed record VerificationSession(
    string SessionId,
    string Purpose,
    OtpChannel Selected,
    string? Phone,      // "+7-901-123-45-67" (для SMS)
    string? Email,
    string Code,        // "1234"
    DateTime ExpiresAt,
    int AttemptsLeft,
    DateTime? NextResendAt
);