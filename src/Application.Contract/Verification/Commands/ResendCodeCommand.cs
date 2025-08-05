using Application.Contract.Verification.Responses;

namespace Application.Contract.Verification.Commands;

public sealed class ResendCodeCommand : IRequest<ResendResponse>
{
    public required string SessionId { get; init; }
}