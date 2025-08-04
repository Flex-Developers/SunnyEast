namespace Application.Contract.Verification.Commands;

public sealed class RequestSmsResult
{
    public int CooldownSeconds { get; init; }
    public int TtlSeconds { get; init; }
}