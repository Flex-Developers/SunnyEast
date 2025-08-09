namespace Application.Contract.Account.Commands;

public sealed class LinkContactCommand : IRequest<Unit>
{
    public required string Channel { get; init; } // "email" | "phone"
    public string? Value { get; init; } // сам e-mail либо +7-XXX-… телефон
}