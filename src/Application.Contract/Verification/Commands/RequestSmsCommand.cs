namespace Application.Contract.Verification.Commands;

public sealed class RequestSmsCommand : IRequest<RequestSmsResult>
{
    public string Phone { get; init; } = default!;
    public string Purpose { get; init; } = "register"; // "register" | "reset"
}
