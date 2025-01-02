using Application.Contract.User.Responses;

namespace Application.Contract.User.Commands;

public class LoginUserCommand : IRequest<JwtTokenResponse>
{
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public required string Password { get; set; }
}