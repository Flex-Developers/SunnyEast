using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Account.Commands;

public sealed class ChangeMyEmailCommandHandler(
    IApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser)
    : IRequestHandler<ChangeEmailCommand, Unit>
{
    public async Task<Unit> Handle(ChangeEmailCommand request, CancellationToken ct)
    {
        var userName = currentUser.GetType().GetMethod("GetUserName")?.Invoke(currentUser, null) as string
                       ?? (currentUser.GetType().GetProperty("UserName")?.GetValue(currentUser) as string);

        if (string.IsNullOrWhiteSpace(userName))
            throw new UnauthorizedAccessException("Пользователь не распознан.");

        var user = await db.Users.FirstAsync(u => u.UserName == userName, ct);

        var newEmail = (request.NewEmail ?? string.Empty).Trim();

        // если e‑mail не меняется – просто выходим
        if (string.Equals(user.Email ?? string.Empty, newEmail, StringComparison.OrdinalIgnoreCase))
            return Unit.Value;

        var exists = await db.Users.AsNoTracking()
            .AnyAsync(u => u.Id != user.Id &&
                           u.Email != null &&
                           u.Email.ToLower() == newEmail.ToLower(), ct);

        if (exists)
            throw new ExistException("Почта уже занята.");

        user.Email = newEmail;
        user.NormalizedEmail = newEmail.ToUpperInvariant();
        // Выход из остальных сессий
        user.SecurityStamp = Guid.NewGuid().ToString();

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}