using Domain.Entities;

namespace Application.Auth;

public interface IEmailConfirmationSender
{
    Task SendConfirmationAsync(ApplicationUser user, CancellationToken ct = default);
    Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct = default);
    Task ResendConfirmationAsync(string email, CancellationToken ct = default);
}
