using System.Text.RegularExpressions;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Account.Commands;

public sealed class ChangeMyPhoneCommandHandler(
    IApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser)
    : IRequestHandler<ChangePhoneCommand, Unit>
{
    public async Task<Unit> Handle(ChangePhoneCommand request, CancellationToken ct)
    {
        var userName = currentUser.GetType().GetMethod("GetUserName")?.Invoke(currentUser, null) as string
                       ?? (currentUser.GetType().GetProperty("UserName")?.GetValue(currentUser) as string);

        if (string.IsNullOrWhiteSpace(userName))
            throw new UnauthorizedAccessException("Пользователь не распознан.");

        var normalized = Normalize(request.NewPhone);
        var exists = await db.Users.AsNoTracking()
            .AnyAsync(u => u.PhoneNumber != null && u.PhoneNumber == normalized, ct);

        if (exists)
            throw new ExistException("Номер телефона уже занят.");

        var user = await userManager.Users.FirstAsync(u => u.UserName == userName, ct);
        user.PhoneNumber = normalized;

        // TODO(прод): здесь — отправка и подтверждение кода по телефону.
        await userManager.UpdateSecurityStampAsync(user); // Выход со всех устройств.
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }

    private static string Normalize(string input)
    {
        // На входе допускаем "+7-XXX-XXX-XX-XX", "8XXXXXXXXXX", "7XXXXXXXXXX", "9XXXXXXXXX"
        var digits = Regex.Replace(input, @"\D", "");
        if (digits.Length == 11 && (digits.StartsWith("8") || digits.StartsWith("7")))
        {
            if (digits.StartsWith("8")) digits = "7" + digits[1..];
        }
        else if (digits.Length == 10 && digits.StartsWith("9"))
        {
            digits = "7" + digits;
        }
        var formatted = Regex.Replace(digits, @"^7(\d{3})(\d{3})(\d{2})(\d{2})$", "+7-$1-$2-$3-$4");
        return formatted;
    }
}