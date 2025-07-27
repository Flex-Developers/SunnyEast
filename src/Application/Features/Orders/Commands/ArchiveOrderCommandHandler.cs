using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Order.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;

namespace Application.Features.Orders.Commands;

public sealed class ArchiveOrderCommandHandler(
    IApplicationDbContext context,
    IHubContext<OrderHub, IOrderClient> hub) : IRequestHandler<ArchiveOrderCommand, Unit>
{
    public async Task<Unit> Handle(ArchiveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.Orders
                        .FirstOrDefaultAsync(o => o.Slug == request.Slug, cancellationToken)
                    ?? throw new NotFoundException($"Заказ {request.Slug} не найден.");

        order.IsInArchive = true;

        await context.SaveChangesAsync(cancellationToken);

        await hub.Clients.Group(OrderHub.ShopGroup(order.ShopId))
            .OrderArchived(order.Slug);

        await hub.Clients.Group(OrderHub.SuperAdminsGroup)
            .OrderArchived(order.Slug);

        await hub.Clients.Group(OrderHub.CustomerGroup(order.CustomerId))
            .OrderArchived(order.Slug);

        return Unit.Value;
    }
}