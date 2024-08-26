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
        var product = await context.Products.FirstOrDefaultAsync(product => product.Slug == request.Slug, cancellationToken);
        if (product == null) 
            throw new NotFoundException($"Product with slug {request.Slug} not found");
        
        product.Name = request.Name ?? product.Name;
        product.Price = request.Price ?? product.Price;
        product.ProductCategorySlug = request.ProductCategorySlug ?? product.ProductCategorySlug;
        product.Description = request.Description?? product.Description;
        
        var category = await context.ProductCategories
            .FirstOrDefaultAsync(s => s.Slug == request.ProductCategorySlug, cancellationToken);
        
        if (category == null)
            throw new NotFoundException($"(Категория не найдена) Category with slug {request.ProductCategorySlug} not found");
        
        product.ProductCategoryId = category.Id;

        if (request.Images != null && request.Images.Length > 0)
            product.Images = request.Images;
        
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}