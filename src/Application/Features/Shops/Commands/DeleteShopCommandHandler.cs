using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Shops.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Shops.Commands;

public class DeleteShopCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteShopCommand,Unit>
{
    public async Task<Unit> Handle(DeleteShopCommand request, CancellationToken cancellationToken)
    {
        var shop = await context.Shops.FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);

        if (shop is not null)
        {
            context.Shops.Remove(shop);
            await context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
        throw new NotFoundException($"Магазин {request.Slug} не найден.");
    }
}