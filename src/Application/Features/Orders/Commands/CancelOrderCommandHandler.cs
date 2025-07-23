using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Order.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public sealed class CancelOrderCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<CancelOrderCommand, Unit>
{
    public async Task<Unit> Handle(CancelOrderCommand req, CancellationToken ct)
    {
        var order = await context.Orders
                        .Include(o => o.OrderItems)
                        .Include(f => f.Customer)
                        .FirstOrDefaultAsync(o => o.Slug == req.Slug, ct)
                    ?? throw new NotFoundException($"Заказ {req.Slug} не найден.");


        var userName = currentUserService.GetUserName() ?? throw new ForbiddenException();

        if (!userName.Equals(order.Customer?.UserName, StringComparison.CurrentCultureIgnoreCase)
            && userName is not "Administrator" && userName is not "Salesman")
            throw new ForbiddenException();


        if (order.Status is not (Domain.Enums.OrderStatus.Submitted or Domain.Enums.OrderStatus.InProgress
            or Domain.Enums.OrderStatus.Ready))
            throw new BadRequestException("Этот заказ уже нельзя отменить.");


        order.Status = Domain.Enums.OrderStatus.Canceled;
        order.CanceledAt = DateTime.UtcNow;

        foreach (var item in order.OrderItems!)
            item.Status = Domain.Enums.OrderStatus.Canceled;

        await context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}