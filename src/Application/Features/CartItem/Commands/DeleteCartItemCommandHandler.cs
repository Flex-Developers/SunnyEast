using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.CartItem.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CartItem.Commands;

public class DeleteCartItemCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteCartItemCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCartItemCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CartItems.FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);

        if (order == null)
            throw new NotFoundException($"Order with slug {request.Slug} is not found");

        context.CartItems.Remove(order);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}