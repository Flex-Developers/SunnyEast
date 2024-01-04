using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Product.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands;

public class UpdateProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateProductCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FirstOrDefaultAsync(c => c.Slug == request.Slug, cancellationToken);
        if (product == null) throw new NotFoundException($"Product with slug {request.Slug}");
        product.Name = request.Name ?? product.Name;
        product.Price = request.Price ?? product.Price;
        product.ProductCategorySlug = request.ProductCategorySlug ?? product.ProductCategorySlug;
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}