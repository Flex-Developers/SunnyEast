using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Identity;
using Application.Contract.Staff.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Staff.Commands;

public sealed class HireUserAsStaffCommandHandler(
    IApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser)
    : IRequestHandler<HireUserAsStaffCommand, Unit>
{
    public async Task<Unit> Handle(HireUserAsStaffCommand request, CancellationToken cancelToken)
    {
        if (currentUser.GetUserId() == request.UserId)
            throw new ForbiddenException("Нельзя назначить самого себя.");

        var userExists = await context.Users.AnyAsync(u => u.Id == request.UserId, cancelToken);

        if (!userExists)
            throw new NotFoundException("Пользователь не найден.");

        var target = await userManager.FindByIdAsync(request.UserId.ToString());
        if (target != null && await userManager.IsInRoleAsync(target, ApplicationRoles.SuperAdmin))
            throw new ForbiddenException("Нельзя изменять статус SuperAdmin.");

        if (await context.Staff.AnyAsync(s => s.UserId == request.UserId, cancelToken))
            return Unit.Value;

        context.Staff.Add(new Domain.Entities.Staff
        {
            UserId = request.UserId,
            StaffRole = Domain.Enums.StaffRole.None,
            IsActive = true,
            ShopId = null
        });

        await context.SaveChangesAsync(cancelToken);
        return Unit.Value;
    }
}