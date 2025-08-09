using Application.Contract.Account.Commands;
using Application.Contract.Account.Responses;
using Application.Contract.User.Responses;
using Application.Contract.Verification.Responses;

namespace Client.Infrastructure.Services.Account;

public interface IAccountService
{
    Task<MyAccountResponse?> GetAsync();
    Task<bool> UpdateProfileAsync(UpdateProfileCommand request);
    Task<bool> ChangeEmailAsync(ChangeEmailCommand request);
    Task<bool> ChangePhoneAsync(ChangePhoneCommand request);
    Task<bool> ChangePasswordAsync(ChangePasswordCommand request);
    Task<bool> DeleteAccountAsync();
    Task<JwtTokenResponse?> RefreshTokenAsync(); // Вспомогательно для «тихого» обновления клейм после успешных операций
    Task<bool> ResetPasswordAsync(string sessionId, string newPassword, string confirm);
    Task<StartVerificationResponse> StartLinkEmailAsync(string newEmail);
    Task<StartVerificationResponse> StartLinkPhoneAsync(string newPhone);
    Task<bool> ConfirmLinkAsync(string sessionId);
}