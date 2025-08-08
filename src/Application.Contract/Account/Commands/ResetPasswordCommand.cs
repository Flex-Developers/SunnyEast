namespace Application.Contract.Account.Commands;

public sealed record ResetPasswordCommand : IRequest<Unit>
{
    public required string SessionId      { get; init; }
    public required string NewPassword    { get; init; }
    public required string ConfirmPassword{ get; init; }
}