using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Product.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands;

public class DeleteProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FirstOrDefaultAsync(s => s.Slug == request.Slug,
            cancellationToken);

        if (product != null)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        throw new NotFoundException($"Продукт не найден {request.Slug}");
    }
}