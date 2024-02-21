using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Order.Commands;
using Application.Contract.Order.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Order.Commands;

public class CreateOrderCommandHandler(
    IApplicationDbContext context,
    ISlugService slugService,
    IMapper mapper,
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<CreateOrderCommand,string>
{
    public async Task<string> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var userName = currentUserService.GetUserName() ?? throw new ForbiddenException();
        var order = mapper.Map<OrderResponse>(request);

       var user = await userManager.FindByNameAsync(userName) ?? throw new ForbiddenException();

       order.Slug = slugService.GenerateSlug(
           $"{request.ShopOrderSlug}-{userName}-{request.CartSlug}");

       var shopId = await context.ShopsOrders
           .Where(s => s.ShopSlug == request.ShopOrderSlug)
           .Select(s => s.Id)
           .FirstOrDefaultAsync(cancellationToken);
       
       if (shopId == Guid.Empty)
           throw new NotFoundException($"The shop with slug {request.ShopOrderSlug} not found.");

       order.ShopOrderSlug = (await context.ShopsOrders.Select(s => s.ShopSlug)
           .FirstOrDefaultAsync(cancellationToken))!;

       order.CartSlug = ((await context.Carts.Select(c => c.Slug).FirstOrDefaultAsync(cancellationToken))!);
       
       // TODO: Complete this
       //order.Quantity = (await context.carts.Select(c => c.))
       
       await context.Orders.AddAsync(order, cancellationToken); // Error
       await context.SaveChangesAsync(cancellationToken);
       return order.Slug;
    }
}