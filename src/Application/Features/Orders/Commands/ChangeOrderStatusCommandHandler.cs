using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Enums;
using Application.Contract.Order.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public sealed class ChangeOrderStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ChangeOrderStatusCommand, Unit>
{
    public async Task<Unit> Handle(ChangeOrderStatusCommand req, CancellationToken ct)
    {
        var order = await context.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefaultAsync(o => o.Slug == req.Slug, ct)
                    ?? throw new NotFoundException($"Order {req.Slug} not found.");

        order.Status = (Domain.Enums.OrderStatus)req.Status;
        order.CustomerComment = req.Comment;

        switch (req.Status)
        {
            case OrderStatus.Ready: order.CanceledAt = null; break;
            case OrderStatus.Issued: order.ClosedAt = DateTime.UtcNow; break;
            case OrderStatus.Canceled: order.CanceledAt = DateTime.UtcNow; break;
        }

        if (order.OrderItems is not null)
            foreach (var i in order.OrderItems)
                i.Status = (Domain.Enums.OrderStatus)req.Status;

        await context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}