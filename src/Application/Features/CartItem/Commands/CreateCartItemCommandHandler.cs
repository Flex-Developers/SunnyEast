using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.CartItem.Commands;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CartItem.Commands;

public class CreateCartItemCommandHandler(
    IApplicationDbContext context,
    ISlugService slugService,
    IMapper mapper,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateCartItemCommand, string>
{
    public async Task<string> Handle(CreateCartItemCommand request, CancellationToken cancellationToken)
    {
        var userName = currentUserService.GetUserName() ?? throw new ForbiddenException();
        var order = mapper.Map<Domain.Entities.CartItem>(request);

        order.Slug = slugService.GenerateSlug(
            $"{request.ShopOrderSlug}-{userName}-{request.CartSlug}");

        var shopId = await context.ShopsOrders
            .Where(s => s.ShopSlug == request.ShopOrderSlug)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (shopId == Guid.Empty)
            throw new NotFoundException($"The shop with slug {request.ShopOrderSlug} not found.");

        order.ProductId = await context.ShopsOrders.Select(s => s.ProductId)
            .FirstOrDefaultAsync(cancellationToken);

        var cart = await context.Carts.FirstOrDefaultAsync(s => s.Slug == request.CartSlug, cancellationToken);
        if (cart == null)
            throw new NotFoundException($"The cart with slug {request.CartSlug} not found.");

        order.CartSlug = cart.Slug;
        order.CartId = cart.Id;

        await context.CartItems.AddAsync(order, cancellationToken); // Error
        await context.SaveChangesAsync(cancellationToken);
        return order.Slug;
    }
}