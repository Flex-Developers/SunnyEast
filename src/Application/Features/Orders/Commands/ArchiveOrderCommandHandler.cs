using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Commands;
using Application.Contract.Order.Responses;
using Application.Contract.Order;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public sealed class ArchiveOrderCommandHandler(
    IApplicationDbContext context,
    IMapper mapper,
    IOrderRealtimeNotifier notifier) : IRequestHandler<ArchiveOrderCommand, Unit>
{
    public async Task<Unit> Handle(ArchiveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.Orders
                        .FirstOrDefaultAsync(o => o.Slug == request.Slug, cancellationToken)
                    ?? throw new NotFoundException($"Заказ {request.Slug} не найден.");

        order.IsInArchive = true;

        await context.SaveChangesAsync(cancellationToken);

        var saved = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Shop)
            .Include(o => o.OrderItems!).ThenInclude(i => i.Product)
            .FirstAsync(o => o.Slug == request.Slug, cancellationToken);

        var response = mapper.Map<OrderResponse>(saved);
        response.Items = mapper.Map<List<OrderItemResponse>>(saved.OrderItems!);
        response.Sum = response.Items.Sum(i => i.SummaryPrice);

        await notifier.OrderArchived(response);

        return Unit.Value;
    }
}