using Application.Contract.User.Commands;

namespace Client.Infrastructure.Services.Auth;

public interface IAuthService
{
    public Task<bool> LoginAsync(LoginUserCommand command, string? returnUrl =  null, bool navigate = true);
    public Task<bool> RegisterAsync(RegisterUserCommand command, string? returnUrl = null);
    Task LogoutAsync(bool navigateToHome = false);
}