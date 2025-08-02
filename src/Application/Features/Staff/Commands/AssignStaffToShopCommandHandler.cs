using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Staff.Commands;
using Application.Contract.Staff.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Staff.Commands;

public sealed class AssignStaffToShopCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AssignStaffToShopCommand, Unit>
{
    public async Task<Unit> Handle(AssignStaffToShopCommand req, CancellationToken ct)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == req.UserId, ct)
                   ?? throw new NotFoundException("Пользователь не найден.");

        var shop = await context.Shops.FirstOrDefaultAsync(s => s.Slug == req.ShopSlug, ct)
                   ?? throw new NotFoundException($"Магазин {req.ShopSlug} не найден.");

        var staff = await context.Staff.FirstOrDefaultAsync(s => s.UserId == user.Id, ct);
        if (staff is null)
        {
            // Нельзя привязывать к магазину, если нет staff-записи с ролью
            staff = new Domain.Entities.Staff
            {
                UserId = user.Id,
                StaffRole = (Domain.Enums.StaffRole)StaffRole.Salesman, // разумный дефолт, если до этого роли не было
                IsActive = true
            };
            await context.Staff.AddAsync(staff, ct);
        }

        if (staff.StaffRole == (Domain.Enums.StaffRole)StaffRole.None)
            throw new BadRequestException("Нельзя привязать к магазину пользователя без staff-роли.");

        staff.ShopId = shop.Id;

        await context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}