using Application.Contract.User.Responses;

namespace Application.Contract.User.Commands;

public class LoginUserCommand : IRequest<JwtTokenResponse>
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}