using Application.Contract.User.Commands;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Identity;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands;

public sealed class DeleteUserCommandHandler(UserManager<ApplicationUser> userManager, IApplicationDbContext context, ICurrentUserService currentUser)
    : IRequestHandler<DeleteUserCommand, Unit>
{
    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.GetUserId() == request.Id) 
            throw new ForbiddenException("Нельзя удалить самого себя.");
        
        // 1) Найдём пользователя
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            throw new NotFoundException($"Пользователь с id={request.Id} не найден.");

        // 2) Защита: нельзя удалить супер-админа
        var roles = await userManager.GetRolesAsync(user);
        if (roles.Contains(ApplicationRoles.SuperAdmin))
            throw new ForbiddenException("Нельзя удалить пользователя с ролью SuperAdmin.");

        // 3) Удаляем связанные записи (если есть)
        var staffRows = await context.Staff.Where(s => s.UserId == user.Id).ToListAsync(cancellationToken);
        if (staffRows.Count > 0)
            context.Staff.RemoveRange(staffRows);

        var subs = await context.NotificationSubscriptions
            .Where(s => s.UserId == user.Id)
            .ToListAsync(cancellationToken);
        if (subs.Count > 0)
            context.NotificationSubscriptions.RemoveRange(subs);

        await context.SaveChangesAsync(cancellationToken); // фиксируем внешние сущности

        // 4) Удаляем самого пользователя из Identity
        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var msg = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Не удалось удалить пользователя: {msg}");
        }

        return Unit.Value;
    }
}
