namespace Application.Common.Interfaces.Services.Otp;

public sealed class OtpOptions
{
    public const string SectionName = "Otp";
    public int Length { get; init; } = 6;
    public int TtlSeconds { get; init; } = 300;
    public int ResendCooldownSeconds { get; init; } = 45;
    public int MaxAttempts { get; init; } = 5;
    public int BlockMinutesOnFail { get; init; } = 10;
}