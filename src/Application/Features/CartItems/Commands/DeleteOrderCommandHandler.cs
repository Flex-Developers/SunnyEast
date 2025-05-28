using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CartItems.Commands;

public class DeleteOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteOrderCommand, Unit>
{
    public async Task<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CartItems.FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);

        if (order == null)
            throw new NotFoundException($"Элемент корзины с id {request.Slug} не найден");

        context.CartItems.Remove(order);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}