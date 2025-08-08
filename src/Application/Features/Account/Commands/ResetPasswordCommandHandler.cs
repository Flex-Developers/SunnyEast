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
    IVerificationSessionStore   store,
    IApplicationDbContext       db,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<ResetPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        // ───── 1. Валидация сессии ────────────────────────────────
        var session = await store.GetAsync(request.SessionId, ct)
                      ?? throw new BadRequestException("Сессия не найдена или истекла.");

        if (!session.IsVerified || session.Purpose != "reset")
            throw new BadRequestException("Код не подтверждён.");

        // ───── 2. Проверка паролей ────────────────────────────────
        if (request.NewPassword != request.ConfirmPassword)
            throw new ValidationException("Пароли не совпадают.");

        if (request.NewPassword.Length < 8
            || !request.NewPassword.Any(char.IsUpper)
            || !request.NewPassword.Any(char.IsLower)
            || !request.NewPassword.Any(char.IsDigit))
            throw new ValidationException("Пароль не удовлетворяет требованиям.");

        // ───── 3. Поиск пользователя ──────────────────────────────
        ApplicationUser? user = null;

        if (!string.IsNullOrWhiteSpace(session.Email))
            user = await db.Users.FirstOrDefaultAsync(u => u.Email == session.Email, ct);

        if (user is null && !string.IsNullOrWhiteSpace(session.Phone))
        {
            // session.Phone хранится как "+7-XXX-XXX-XX-XX" → берём 4 последние цифры
            var targetDigits = Digits(session.Phone);                  // "79011234567"
            var last4        = targetDigits[^4..];                     // "4567"

            // сначала узкий предикат по EndsWith(last4) – легко переводится в SQL
            var candidates = await db.Users
                .Where(u => u.PhoneNumber != null && u.PhoneNumber.EndsWith(last4))
                .ToListAsync(ct);

            // затем точное совпадение уже в памяти
            user = candidates.FirstOrDefault(u => Digits(u.PhoneNumber) == targetDigits);
        }

        if (user is null)
            throw new NotFoundException("Пользователь не найден.");

        // ───── 4. Смена пароля ────────────────────────────────────
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var res   = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        res.ThrowBadRequestIfError();

        await userManager.UpdateSecurityStampAsync(user); // инвалидируем старые JWT
        await store.RemoveAsync(session.SessionId, ct);   // чистим верификационную сессию

        return Unit.Value;
    }

    /// <summary>Возвращает только цифры исходной строки (телефон).</summary>
    private static string Digits(string? s) => Regex.Replace(s ?? "", @"\D", "");
}
