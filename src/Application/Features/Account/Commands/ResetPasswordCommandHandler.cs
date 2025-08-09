using System.ComponentModel.DataAnnotations;
using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Account.Commands;

public sealed class ResetPasswordCommandHandler(
    IVerificationSessionStore store,
    UserManager<ApplicationUser> userManager) 
    : IRequestHandler<ResetPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var session = await store.GetAsync(request.SessionId, ct)
                      ?? throw new BadRequestException("Сессия не найдена или истекла.");

        if (!session.IsVerified || session.Purpose != "reset")
            throw new BadRequestException("Код не подтверждён.");

        if (request.NewPassword != request.ConfirmPassword)
            throw new ValidationException("Пароли не совпадают.");
        if (request.NewPassword.Length < 8
            || !request.NewPassword.Any(char.IsUpper)
            || !request.NewPassword.Any(char.IsLower)
            || !request.NewPassword.Any(char.IsDigit))
            throw new ValidationException("Пароль не удовлетворяет требованиям.");

        // --- 1) Находим пользователя через UserManager.Users ---
        ApplicationUser? found = null;

        if (!string.IsNullOrWhiteSpace(session.Email))
        {
            found = await userManager.Users.FirstOrDefaultAsync(u => u.Email == session.Email, ct);
        }

        if (found is null && !string.IsNullOrWhiteSpace(session.Phone))
        {
            var targetE164 = Application.Common.Utils.PhoneMasking.NormalizeE164(session.Phone);
            var last4 = new string(targetE164.Where(char.IsDigit).ToArray())[^4..];

            var candidates = await userManager.Users
                .Where(u => u.PhoneNumber != null && u.PhoneNumber.EndsWith(last4))
                .ToListAsync(ct);

            found = candidates.FirstOrDefault(u =>
                Application.Common.Utils.PhoneMasking.NormalizeE164(u.PhoneNumber!) == targetE164);
        }

        if (found is null)
            throw new NotFoundException("Пользователь не найден.");

        // --- 2) Берём «трекаемый» инстанс напрямую из UserManager ---
        var user = await userManager.FindByIdAsync(found.Id.ToString())
                   ?? throw new NotFoundException("Пользователь не найден.");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var res   = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        res.ThrowBadRequestIfError();

        await userManager.UpdateSecurityStampAsync(user);
        await store.RemoveAsync(session.SessionId, ct);

        return Unit.Value;
    }
}
