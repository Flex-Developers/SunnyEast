using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Order.Commands;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public class CreateOrderCommandHandler(
    IApplicationDbContext context,
    ISlugService slugService,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateOrderCommand, string>
{
    public async Task<string> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var userName = currentUserService.GetUserName() ?? throw new ForbiddenException();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Name == userName, cancellationToken)
                   ?? throw new ForbiddenException();

        var shop = await context.Shops.FirstOrDefaultAsync(s => s.Slug == request.ShopSlug, cancellationToken);
        if (shop == null)
            throw new NotFoundException($"The shop with slug {request.ShopSlug} not found.");

        var order = new Domain.Entities.Order
        {
            Slug = slugService.GenerateSlug($"{request.ShopSlug}-{userName}-{Guid.NewGuid()}"),
            ShopId = shop.Id,
            ShopSlug = shop.Slug,
            CustomerId = user.Id,
            Status = OrderStatus.Submited,
            OrderItems = new List<Domain.Entities.OrderItem>()
        };

        foreach (var item in request.Items)
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Slug == item.ProductSlug, cancellationToken)
                          ?? throw new NotFoundException($"Product {item.ProductSlug} not found.");

            var price = product.DiscountPrice ?? product.Price;
            var summary = price * item.Quantity;

            order.OrderItems.Add(new Domain.Entities.OrderItem
            {
                OrderSlug = order.Slug,
                ProductId = product.Id,
                ProductSlug = product.Slug,
                Quantity = item.Quantity,
                SummaryPrice = summary,
                Status = OrderStatus.Submited
            });
        }

        await context.Orders.AddAsync(order, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return order.Slug;
    }
}
