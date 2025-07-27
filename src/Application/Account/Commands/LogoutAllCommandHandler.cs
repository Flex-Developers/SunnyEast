using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Account.Commands;

public sealed class LogoutAllCommandHandler(
    IApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser)
    : IRequestHandler<LogoutAllCommand, Unit>
{
    public async Task<Unit> Handle(LogoutAllCommand request, CancellationToken ct)
    {
        var userName = currentUser.GetType().GetMethod("GetUserName")?.Invoke(currentUser, null) as string
                       ?? (currentUser.GetType().GetProperty("UserName")?.GetValue(currentUser) as string);

        if (string.IsNullOrWhiteSpace(userName))
            throw new UnauthorizedAccessException("Пользователь не распознан.");

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == userName, ct)
                   ?? throw new NotFoundException("Пользователь не найден.");

        await userManager.UpdateSecurityStampAsync(user);
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}