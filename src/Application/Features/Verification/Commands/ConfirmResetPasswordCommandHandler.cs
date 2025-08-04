using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services.Otp;
using Application.Contract.Verification.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Features.Verification.Commands;

public sealed class ConfirmResetPasswordCommandHandler(
    IApplicationDbContext db,
    IOtpService otp,
    IOptions<OtpOptions> options,
    UserManager<ApplicationUser> userManager) : IRequestHandler<ConfirmResetPasswordCommand, bool>
{
    private readonly OtpOptions _opt = options.Value;

    public async Task<bool> Handle(ConfirmResetPasswordCommand request, CancellationToken ct)
    {
        var phone = otp.NormalizePhoneE164(request.Phone);
        var now = DateTimeOffset.UtcNow;

        var rec = await db.PhoneOtps
            .Where(x => x.PhoneE164 == phone && x.Purpose == "reset")
            .OrderByDescending(x => x.LastSentAt)
            .FirstOrDefaultAsync(ct);

        if (rec is null || rec.ExpiresAt < now)
            throw new InvalidOperationException("Код истёк. Запросите новый.");

        if (!otp.Verify(request.Code, rec.CodeHash, rec.Salt))
        {
            rec.Attempts++;
            if (rec.Attempts >= _opt.MaxAttempts)
                rec.BlockedUntil = now.AddMinutes(_opt.BlockMinutesOnFail);

            await db.SaveChangesAsync(ct);
            throw new InvalidOperationException("Неверный код.");
        }

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone, ct);
        if (user is null)
            throw new InvalidOperationException("Пользователь не найден.");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var res = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!res.Succeeded)
            throw new InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));

        return true;
    }
}