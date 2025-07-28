using Application.Contract.Account.Commands;
using Application.Contract.Account.Responses;
using Application.Contract.User.Responses;

namespace Client.Infrastructure.Services.Account;

public interface IAccountService
{
    Task<MyAccountResponse?> GetAsync();

    Task<bool> UpdateProfileAsync(UpdateProfileCommand request);

    Task<bool> ChangeEmailAsync(ChangeEmailCommand request);

    Task<bool> ChangePhoneAsync(ChangePhoneCommand request);

    Task<bool> ChangePasswordAsync(ChangePasswordCommand request);
    
    Task<bool> DeleteAccountAsync();

    // Вспомогательно для «тихого» обновления клейм после успешных операций
    Task<JwtTokenResponse?> RefreshTokenAsync();
}