using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.CartItem.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CartItem.Commands;

public class UpdateCartItemCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateCartItemCommand, string>
{
    public async Task<string> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CartItems.FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);
        
        if (order == null) 
            throw new NotFoundException($"Order with id {request.Slug} is not found");
        
        order.Quantity = request.Quantity;
        
        await context.SaveChangesAsync(cancellationToken);
        return order.Slug;
    }
}