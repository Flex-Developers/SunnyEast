using Application.Contract.User.Commands;

namespace Client.Infrastructure.Services.Auth;

public interface IAuthService
{
    public Task<bool> LoginAsync(LoginUserCommand command);
    Task LogoutAsync();
    public Task<bool> IsUsernameExistsAsync(string username);
    public Task<bool> IsEmailExistsAsync(string email);
}