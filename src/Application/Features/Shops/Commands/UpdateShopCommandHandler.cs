using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Shops.Commands;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Shops.Commands;

public class UpdateShopCommandHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<UpdateShopCommand,Unit>
{
    public async Task<Unit> Handle(UpdateShopCommand request, CancellationToken cancellationToken)
    {
        var shop = await context.Shops.FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);

        if (shop is null)
            throw new NotFoundException($"Магазин {request.Address} не найден");

        mapper.Map(request, shop);
        
        if (request.Images is not null)
            shop.Images = request.Images.Where(i => !string.IsNullOrWhiteSpace(i)).ToArray();

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}