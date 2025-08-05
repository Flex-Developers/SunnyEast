using Application.Contract.Verification.Responses;

namespace Application.Contract.Verification.Queries;

public sealed class GetSessionStateQuery : IRequest<GetSessionStateResponse>
{
    public required string SessionId { get; init; }
}