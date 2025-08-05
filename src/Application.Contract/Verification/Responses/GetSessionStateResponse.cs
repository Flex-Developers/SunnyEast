using Application.Contract.Verification.Enums;

namespace Application.Contract.Verification.Responses;

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