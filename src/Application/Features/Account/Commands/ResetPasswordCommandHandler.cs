using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Account.Commands;

public sealed class ResetPasswordCommandHandler(
    IVerificationSessionStore store,
    IApplicationDbContext db,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<ResetPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        // 1) Сессия
        var session = await store.GetAsync(request.SessionId, ct)
                      ?? throw new BadRequestException("Сессия не найдена или истекла.");
        if (!session.IsVerified || session.Purpose != "reset")
            throw new BadRequestException("Код не подтверждён.");

        // 2) Пароль
        if (request.NewPassword != request.ConfirmPassword)
            throw new ValidationException("Пароли не совпадают.");
        if (request.NewPassword.Length < 8
            || !request.NewPassword.Any(char.IsUpper)
            || !request.NewPassword.Any(char.IsLower)
            || !request.NewPassword.Any(char.IsDigit))
            throw new ValidationException("Пароль должен содержать минимум 8 символов, включая прописные и строчные буквы и цифры.");

        // 3) Ищем пользователя → получаем ТОЛЬКО Id без трекинга
        Guid? userId = null;

        if (!string.IsNullOrWhiteSpace(session.Email))
        {
            userId = await db.Users.AsNoTracking()
                .Where(u => u.Email == session.Email)
                .Select(u => (Guid?)u.Id)
                .FirstOrDefaultAsync(ct);
        }

        if (userId is null && !string.IsNullOrWhiteSpace(session.Phone))
        {
            // В сессии телефон хранится в E.164 (+7XXXXXXXXXX) — сравниваем по цифрам
            var target = Digits(session.Phone);
            var last4  = target[^4..];

            var candidates = await db.Users.AsNoTracking()
                .Where(u => u.PhoneNumber != null && u.PhoneNumber.EndsWith(last4))
                .Select(u => new { u.Id, u.PhoneNumber })
                .ToListAsync(ct);

            userId = candidates
                .FirstOrDefault(c => Digits(c.PhoneNumber!) == target)?
                .Id;
        }

        if (userId is null)
            throw new NotFoundException("Пользователь не найден.");

        // 4) Повторно берём пользователя уже через UserManager (его контекст!)
        var user = await userManager.FindByIdAsync(userId.ToString()!)
                   ?? throw new NotFoundException("Пользователь не найден.");

        // 5) Смена пароля
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var res   = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        res.ThrowBadRequestIfError();

        await userManager.UpdateSecurityStampAsync(user); // инвалидируем остальные логины (если проверяется)
        await store.RemoveAsync(session.SessionId, ct);

        return Unit.Value;
    }

    private static string Digits(string? s) => Regex.Replace(s ?? "", @"\D", "");
}