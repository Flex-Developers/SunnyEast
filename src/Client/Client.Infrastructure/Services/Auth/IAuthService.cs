using Application.Contract.User.Commands;

namespace Client.Infrastructure.Services.Auth;

public interface IAuthService
{
    public Task<bool> LoginAsync(LoginUserCommand command, string? returnUrl =  null);
    public Task<bool> RegisterAsync(RegisterUserCommand command, string? returnUrl = null);
    Task ResendConfirmationAsync(string email);
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task LogoutAsync(bool navigateToHome = false);
}