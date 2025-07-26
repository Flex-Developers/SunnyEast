using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Identity;
using Application.Contract.Staff.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Staff.Commands;

public sealed class DeleteStaffCommandHandler(IApplicationDbContext context, UserManager<ApplicationUser> userManager)
    : IRequestHandler<DeleteStaffCommand, Unit>
{
    public async Task<Unit> Handle(DeleteStaffCommand req, CancellationToken ct)
    {
        // 1) Пользователь
        var user = await userManager.FindByIdAsync(req.UserId.ToString())
                   ?? throw new NotFoundException("Пользователь не найден.");

        if (await userManager.IsInRoleAsync(user, ApplicationRoles.SuperAdmin))
            throw new ForbiddenException("Нельзя удалять супер-администратора.");

        // 2) Снять staff-роли (если есть)
        var currentRoles = await userManager.GetRolesAsync(user);
        var toRemove = currentRoles
            .Where(r => r.Equals(ApplicationRoles.Administrator, StringComparison.OrdinalIgnoreCase)
                        || r.Equals(ApplicationRoles.Salesman, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (toRemove.Length > 0)
        {
            var res = await userManager.RemoveFromRolesAsync(user, toRemove);
            res.ThrowBadRequestIfError();
        }

        // (опционально) инвалидировать cookie‑сессии
        await userManager.UpdateSecurityStampAsync(user);

        // 3) Удалить запись Staff (если есть)
        var staff = await context.Staff.FirstOrDefaultAsync(s => s.UserId == req.UserId, ct);
        if (staff != null)
        {
            context.Staff.Remove(staff);
            await context.SaveChangesAsync(ct);
        }

        // Возвращаем 204 в любом случае (идемпотентность)
        return Unit.Value;
    }
}