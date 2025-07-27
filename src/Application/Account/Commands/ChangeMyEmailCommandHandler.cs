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

        var normalized = request.NewEmail.Trim();
        var exists = await db.Users.AsNoTracking()
            .AnyAsync(u => u.Email != null && u.Email.ToLower() == normalized.ToLower(), ct);

        if (exists)
            throw new ExistException("Почта уже занята.");

        var user = await userManager.Users.FirstAsync(u => u.UserName == userName, ct);
        var setEmailRes = await userManager.SetEmailAsync(user, normalized);
        setEmailRes.ThrowBadRequestIfError();

        // TODO(прод): здесь должен быть процесс подтверждения e‑mail кодом.
        await userManager.UpdateSecurityStampAsync(user); // Выход со всех устройств, как и требовали.
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}