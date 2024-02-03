using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Cart.Commands;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using AutoMapper;
using MediatR;

namespace Application.Features.Cart.Commands;

public class CreateCartCommandHandler(IApplicationDbContext context, IMapper mapper, ISlugService slugService)
: IRequestHandler<CreateCartCommand,string>
{
    public async Task<string> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var cart = mapper.Map<Domain.Entities.Cart>(request);
        
        cart.Slug = slugService.GenerateSlug(request.ShopSlug);
        cart.CustomerId = Guid.Empty;

        if (await context.Carts.AnyAsync(c => c.Slug == cart.Slug, cancellationToken))
            throw new ExistException($"The cart with slug '{cart.Slug}' already exists");

        await context.Carts.AddAsync(cart, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return cart.Slug;
    }
}