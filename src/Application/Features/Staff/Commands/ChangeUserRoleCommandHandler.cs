using System.ComponentModel.DataAnnotations;
using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Identity;
using Application.Contract.Staff.Commands;
using Application.Contract.Staff.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Staff.Commands;

public sealed class ChangeUserRoleCommandHandler(
    IApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<ChangeUserRoleCommand, Unit>
{
    public async Task<Unit> Handle(ChangeUserRoleCommand req, CancellationToken ct)
    {
        // Получаем пользователя ТОЛЬКО через UserManager
        var user = await userManager.FindByIdAsync(req.UserId.ToString())
                   ?? throw new NotFoundException("Пользователь не найден.");

        if (await userManager.IsInRoleAsync(user, ApplicationRoles.SuperAdmin))
            throw new ForbiddenException("Изменение роли SuperAdmin запрещено этой операцией.");

        // Сначала снимаем staff-роли
        await RemoveIfInRoleAsync(user, ApplicationRoles.Administrator);
        await RemoveIfInRoleAsync(user, ApplicationRoles.Salesman);

        // Потом ставим нужную
        switch (req.Role)
        {
            case StaffRole.Administrator:
                await EnsureInRoleAsync(user, ApplicationRoles.Administrator);
                break;
            case StaffRole.Salesman:
                await EnsureInRoleAsync(user, ApplicationRoles.Salesman);
                break;
            case StaffRole.None:
                break;
            default:
                throw new ValidationException("Недопустимое значение StaffRole.");
        }

        // Обновляем запись Staff через свой контекст (ApplicationUser сюда не прикрепляем)
        var staff = await context.Staff.FirstOrDefaultAsync(s => s.UserId == req.UserId, ct);
        if (staff is null)
        {
            staff = new Domain.Entities.Staff
            {
                UserId    = req.UserId,
                IsActive  = true,
                StaffRole = (Domain.Enums.StaffRole)req.Role,
                ShopId    = null
            };
            await context.Staff.AddAsync(staff, ct);
        }
        else
        {
            staff.StaffRole = (Domain.Enums.StaffRole)req.Role;
            if (req.Role == StaffRole.None)
            {
                staff.ShopId  = null;
                staff.IsActive = false;
            }
        }
        await userManager.UpdateSecurityStampAsync(user);
        await context.SaveChangesAsync(ct);
        return Unit.Value;
    }

    private async Task EnsureInRoleAsync(ApplicationUser user, string role)
        => (await userManager.AddToRoleAsync(user, role)).ThrowBadRequestIfError();

    private async Task RemoveIfInRoleAsync(ApplicationUser user, string role)
    {
        if (await userManager.IsInRoleAsync(user, role))
            (await userManager.RemoveFromRoleAsync(user, role)).ThrowBadRequestIfError();
    }
}
