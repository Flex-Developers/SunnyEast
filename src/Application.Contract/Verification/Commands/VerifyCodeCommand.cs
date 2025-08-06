using Application.Contract.Verification.Responses;

namespace Application.Contract.Verification.Commands;

public sealed class VerifyCodeCommand : IRequest<VerifyResponse>
{
    public required string SessionId { get; init; }
    public required string Code { get; init; }
}
