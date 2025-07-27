using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Commands;
using Application.Contract.Order.Responses;
using Application.Contract.Realtime;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public sealed class ArchiveOrderCommandHandler(
    IApplicationDbContext context,
    IMapper mapper,
    IOrderRealtimeNotifier realtimeNotifier) : IRequestHandler<ArchiveOrderCommand, Unit>
{
    public async Task<Unit> Handle(ArchiveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.Orders
                        .FirstOrDefaultAsync(o => o.Slug == request.Slug, cancellationToken)
                    ?? throw new NotFoundException($"Заказ {request.Slug} не найден.");

        order.IsInArchive = true;

        await context.SaveChangesAsync(cancellationToken);

        var response = mapper.Map<OrderResponse>(order);
        await realtimeNotifier.OrderArchived(response);

        return Unit.Value;
    }
}