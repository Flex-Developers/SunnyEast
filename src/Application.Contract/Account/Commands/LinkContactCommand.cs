using Application.Contract.Verification.Responses;

namespace Application.Contract.Account.Commands;

public sealed class LinkContactCommand : IRequest<StartVerificationResponse>
{
    public required string Channel { get; init; } // "email" | "phone"
    public string? Value { get; init; } // сам e-mail либо +7-XXX-… телефон
}