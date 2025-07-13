using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Commands;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public class UpdateOrderStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateOrderStatusCommand, Unit>
{
    public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Slug == request.Slug, cancellationToken);

        if (order == null)
            throw new NotFoundException($"Order with slug {request.Slug} not found.");

        order.Status = request.Status;
        if (order.OrderItems is not null)
        {
            foreach (var item in order.OrderItems)
            {
                item.Status = request.Status;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
