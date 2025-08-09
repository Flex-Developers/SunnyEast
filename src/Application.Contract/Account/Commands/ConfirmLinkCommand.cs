namespace Application.Contract.Account.Commands;

public sealed class ConfirmLinkCommand : IRequest<Unit>
{
    public required string SessionId { get; init; }
}