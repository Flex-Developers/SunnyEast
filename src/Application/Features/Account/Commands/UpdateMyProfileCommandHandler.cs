using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Account.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Account.Commands;

public sealed class UpdateMyProfileCommandHandler(
    IApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateProfileCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var userName = currentUser.GetType().GetMethod("GetUserName")?.Invoke(currentUser, null) as string
                       ?? (currentUser.GetType().GetProperty("UserName")?.GetValue(currentUser) as string);

        if (string.IsNullOrWhiteSpace(userName))
            throw new UnauthorizedAccessException("Пользователь не распознан.");

        // ВАЖНО: грузим и сохраняем через один и тот же DbContext
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == userName, ct)
                   ?? throw new NotFoundException("Пользователь не найден.");

        user.Name = request.Name.Trim();
        user.Surname = request.Surname.Trim();

        await db.SaveChangesAsync(ct);
        // ВАЖНО: security stamp при смене имени НЕ обновляем (не выходим из сессий).
        // Токен обновим отдельным запросом /api/account/refresh-token при необходимости.
        return Unit.Value;
    }
}