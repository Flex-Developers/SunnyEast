using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Staff.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Staff.Commands;


public sealed class HireUserAsStaffCommandHandler(IApplicationDbContext context)
    : IRequestHandler<HireUserAsStaffCommand, Unit>
{
    public async Task<Unit> Handle(HireUserAsStaffCommand request, CancellationToken cancelToken)
    {
        var userExists = await context.Users.AnyAsync(u => u.Id == request.UserId, cancelToken);
        
        if (!userExists)
            throw new NotFoundException("Пользователь не найден.");

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