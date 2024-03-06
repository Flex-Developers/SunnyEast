using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Order.Commands;

public class DeleteOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteOrderCommand, Unit>
{
    public async Task<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.Orders.FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);

        if (order == null)
            throw new NotFoundException($"Order with slug {request.Slug} is not found");

        context.Orders.Remove(order);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}