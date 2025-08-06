using Application.Contract.User.Commands;

namespace Client.Infrastructure.Services.Verification.Register;

public interface IRegistrationDraftService
{
    Task SavePublicAsync(RegisterUserCommand publicPart);
    Task<RegisterUserCommand?> GetPublicAsync();
    Task ClearPublicAsync();

    void SetPassword(string password, string confirm);
    (string? Password, string? Confirm) GetPassword();
    void ClearPassword();
    Task ClearAllAsync();
}