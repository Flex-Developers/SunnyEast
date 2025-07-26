using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Staff.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Staff.Commands;

public sealed class SetStaffActiveCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SetStaffActiveCommand, Unit>
{
    public async Task<Unit> Handle(SetStaffActiveCommand req, CancellationToken ct)
    {
        var staff = await context.Staff.FirstOrDefaultAsync(s => s.UserId == req.UserId, ct)
                    ?? throw new NotFoundException("Сотрудник не найден.");

        staff.IsActive = req.IsActive;
        await context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}