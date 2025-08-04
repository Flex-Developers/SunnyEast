namespace Application.Contract.Verification.Commands;

public sealed class ConfirmResetPasswordCommand : IRequest<bool>
{
    public string Phone { get; init; } = default!;
    public string Code  { get; init; } = default!;
    public string NewPassword { get; init; } = default!;
}