using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Order.Commands;
using Application.Contract.Order.Responses;
using AutoMapper;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public class CreateOrderCommandHandler(
    IApplicationDbContext context,
    ISlugService slugService,
    ICurrentUserService currentUserService,
    IDateTimeService dateTimeService,
    IMapper mapper,
    IHubContext<OrderHub, IOrderClient> hub)
    : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var userName = currentUserService.GetUserName() ?? throw new ForbiddenException();

        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken)
                   ?? throw new ForbiddenException();

        var shop = await context.Shops.FirstOrDefaultAsync(s => s.Slug == request.ShopSlug, cancellationToken);
        if (shop == null)
            throw new NotFoundException($"Магазин {request.ShopSlug} не найден.");

        var order = new Domain.Entities.Order
        {
            Slug = slugService.GenerateSlug($"{request.ShopSlug}-{userName}-{Guid.NewGuid()}"),
            ShopId = shop.Id,
            ShopSlug = shop.Slug,
            CustomerId = user.Id,
            Customer = user,
            CreatedAt = dateTimeService.Moscow,
            Status = OrderStatus.Submitted,
            OrderItems = new List<Domain.Entities.OrderItem>(),
            OrderNumber = await GenerateOrderNumberAsync(cancellationToken),
            CustomerComment = request.Comment
        };

        foreach (var item in request.Items)
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Slug == item.ProductSlug, cancellationToken)
                          ?? throw new NotFoundException($"Продукт {item.ProductSlug} не найден.");

            var price = product.DiscountPrice ?? product.Price;
            var summary = price * item.Quantity;

            order.OrderItems.Add(new Domain.Entities.OrderItem
            {
                OrderSlug = order.Slug,
                ProductId = product.Id,
                ProductSlug = product.Slug,
                Quantity = item.Quantity,
                SummaryPrice = summary,
                Volume = item.SelectedVolume,
                Status = OrderStatus.Submitted
            });
        }

        await context.Orders.AddAsync(order, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var ord = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Shop)
            .Include(o => o.OrderItems)
            .Where(o => o.Id == order.Id)
            .Select(o => mapper.Map<Application.Contract.Order.Responses.OrderResponse>(o))
            .FirstAsync(cancellationToken);

        await hub.Clients.Group(OrderHub.ShopGroup(order.ShopId)).OrderCreated(ord);
        await hub.Clients.Group(OrderHub.SuperAdminsGroup).OrderCreated(ord);

        return new CreateOrderResponse
        {
            Slug = order.Slug,
            OrderNumber = order.OrderNumber,
        };
    }
    
    private async Task<string> GenerateOrderNumberAsync(CancellationToken ct)
    {
        string number;
        do
        {
            number = $"{dateTimeService.Moscow:yyyy-MM-dd}-{Random.Shared.Next(0, 10000):D4}";
        }
        while (await context.Orders.AnyAsync(o => o.OrderNumber == number, ct));

        return number;
    }

}
