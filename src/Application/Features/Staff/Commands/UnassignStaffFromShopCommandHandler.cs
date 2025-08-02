using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Staff.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Staff.Commands;

public sealed class UnassignStaffFromShopCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UnassignStaffFromShopCommand, Unit>
{
    public async Task<Unit> Handle(UnassignStaffFromShopCommand req, CancellationToken ct)
    {
        var staff = await context.Staff.FirstOrDefaultAsync(s => s.UserId == req.UserId, ct)
                    ?? throw new NotFoundException("Сотрудник не найден.");

        staff.ShopId = null;
        await context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}