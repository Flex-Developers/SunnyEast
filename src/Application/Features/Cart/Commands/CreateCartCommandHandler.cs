using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Cart.Commands;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Cart.Commands;

public class CreateCartCommandHandler(
    IApplicationDbContext context,
    IMapper mapper,
    ISlugService slugService,
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<CreateCartCommand, string>
{
    public async Task<string> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var username = currentUserService.GetUserName() ?? throw new ForbiddenException();
        var cart = mapper.Map<Domain.Entities.Cart>(request);

        var user = await userManager.FindByNameAsync(username) ?? throw new ForbiddenException();

        if (await context.Carts.AnyAsync(s => s.CustomerId == user.Id && s.Status == OrderStatus.Opened,
                cancellationToken))
        {
            throw new ExistException("You have opened cart");
        }

        cart.Slug = slugService.GenerateSlug(
            $"{request.ShopSlug}-{username}-{await context.Carts.CountAsync(s => s.CustomerId == user.Id, cancellationToken)}");
        var shopId = await context.Shops.Where(s => s.Slug == request.ShopSlug).Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (shopId == Guid.Empty)
        {
            throw new NotFoundException($"The shop with slug {request.ShopSlug} not found");
        }

        cart.ShopId = shopId;
        cart.CustomerId = user.Id;

        await context.Carts.AddAsync(cart, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return cart.Slug;
    }
}