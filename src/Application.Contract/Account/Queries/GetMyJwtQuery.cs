using Application.Contract.User.Responses;

namespace Application.Contract.Account.Queries;

public sealed class GetMyJwtQuery : IRequest<JwtTokenResponse>
{
}