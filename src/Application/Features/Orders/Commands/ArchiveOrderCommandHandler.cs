using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public sealed class ArchiveOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<ArchiveOrderCommand, Unit>
{
    public async Task<Unit> Handle(ArchiveOrderCommand req, CancellationToken ct)
    {
        var order = await context.Orders
                        .FirstOrDefaultAsync(o => o.Slug == req.Slug, ct)
                    ?? throw new NotFoundException($"Заказ {req.Slug} не найден.");

        order.IsInArchive = req.IsInArchive; 
        
        await context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}