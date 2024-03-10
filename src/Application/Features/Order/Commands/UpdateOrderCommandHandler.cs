using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Order.Commands;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Order.Commands;

public class UpdateOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateOrderCommand, string>
{
    public async Task<string> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.Orders.FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);
        
        if (order == null) 
            throw new NotFoundException($"Order with id {request.Slug} is not found");
        
        order.Quantity = request.Quantity;
        
        await context.SaveChangesAsync(cancellationToken);
        return order.Slug;
    }
}