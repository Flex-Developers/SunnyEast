using Application.Contract.User.Commands;
using Application.Contract.User.Responses;

namespace Client.Infrastructure.Services.Auth;

public interface IAuthService
{
    public Task<bool> LoginAsync(LoginUserCommand command, string? returnUrl =  null);
    public Task<bool> RegisterAsync(RegisterUserCommand command, string? returnUrl = null);
    Task LogoutAsync(bool navigateToHome = false);
    public Task LoginWithTokenAsync(JwtTokenResponse token, string? returnUrl = null);
}