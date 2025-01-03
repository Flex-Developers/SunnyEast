﻿using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Shops.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Shops.Commands;

public class UpdateShopCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateShopCommand,Unit>
{
    public async Task<Unit> Handle(UpdateShopCommand request, CancellationToken cancellationToken)
    {
        var shop = await context.Shops.FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);

        if (shop is null)
            throw new NotFoundException($"Shop with slug {request.Slug} not found");

        shop.Address = request.Address;
        shop.Images = request.Images;
        shop.AddressGoogle = request.AddressGoogle ?? null;
        shop.AddressYandex = request.AddressYandex ?? null;

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}