namespace Application.Common.Interfaces.Services;

public interface IUserService
{
    public Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken);
    public Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken);
    public Task<bool> IsPhoneNumberExistsAsync(string phoneNumber, CancellationToken cancellationToken);
}