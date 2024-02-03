using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Cart.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Cart.Commands;

public class DeleteCartCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteCartCommand,Unit>
{
    public async Task<Unit> Handle(DeleteCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await context.Carts.FirstOrDefaultAsync(c => c.Slug == request.Slug,cancellationToken);

        if (cart is not null)
        {
            context.Carts.Remove(cart);
            await context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        throw new NotFoundException($"Cart with slug {request.Slug} not found");
    }
}