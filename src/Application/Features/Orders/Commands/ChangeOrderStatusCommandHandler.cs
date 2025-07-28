using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Enums;
using Application.Contract.Order.Commands;
using Application.Contract.Order.Responses;
using MediatR;
using AutoMapper;
using Application.Contract.Order;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public sealed class ChangeOrderStatusCommandHandler(
    IApplicationDbContext context,
    IMapper mapper,
    IOrderRealtimeNotifier notifier)
    : IRequestHandler<ChangeOrderStatusCommand, Unit>
{
    public async Task<Unit> Handle(ChangeOrderStatusCommand req, CancellationToken ct)
    {
        var order = await context.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefaultAsync(o => o.Slug == req.Slug, ct)
                    ?? throw new NotFoundException($"Order {req.Slug} not found.");

        order.Status = (Domain.Enums.OrderStatus)req.Status;

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

        var saved = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Shop)
            .Include(o => o.OrderItems!).ThenInclude(i => i.Product)
            .FirstAsync(o => o.Slug == req.Slug, ct);

        var response = mapper.Map<OrderResponse>(saved);
        response.Items = mapper.Map<List<OrderItemResponse>>(saved.OrderItems!);
        response.Sum = response.Items.Sum(i => i.SummaryPrice);

        await notifier.OrderStatusChanged(response);

        return Unit.Value;
    }
}