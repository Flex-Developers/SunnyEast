using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Shops.Commands;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Shops.Commands;

public class CreateShopCommandHandler(IApplicationDbContext context, IMapper mapper, ISlugService slugService)
: IRequestHandler<CreateShopCommand, string>
{
    public async Task<string> Handle(CreateShopCommand request, CancellationToken cancellationToken)
    {
        var shop = mapper.Map<Shop>(request);

        shop.Slug = slugService.GenerateSlug(shop.Address);
        
        if (await context.Shops.AnyAsync(s => s.Slug == shop.Slug, cancellationToken))
            throw new ExistException($"Магазин с адресом {shop.Address} уже существует");
        
        shop.Images = shop.Images?.Where(i => !string.IsNullOrWhiteSpace(i)).ToArray();

        await context.Shops.AddAsync(shop, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return shop.Slug;
    }
}