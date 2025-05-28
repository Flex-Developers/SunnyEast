using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CartItems.Commands;

public class UpdateOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateOrderCommand, string>
{
    public async Task<string> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CartItems.FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);

        if (order == null)
            throw new NotFoundException($"Order with id {request.Slug} is not found");

        order.Quantity = request.Quantity;

        await context.SaveChangesAsync(cancellationToken);
        return order.Slug;
    }
}