using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Product.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands;

public class UpdateProductCommandHandler(IApplicationDbContext context, ISlugService slugService)
    : IRequestHandler<UpdateProductCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FirstOrDefaultAsync(product => product.Slug == request.Slug, cancellationToken);
        
        if (product is null) 
            throw new NotFoundException($"Продукт не найден {request.Slug}");
        
        product.Name = request.Name ?? product.Name;
        product.Price = request.Price ?? product.Price;
        product.DiscountPrice = request.DiscountPrice?? product.DiscountPrice;
        product.DiscountPercentage = request.DiscountPercentage?? product.DiscountPercentage;
        product.ProductCategorySlug = request.ProductCategorySlug ?? product.ProductCategorySlug;
        product.Description = request.Description?? product.Description;
        
        var category = await context.ProductCategories
            .FirstOrDefaultAsync(s => s.Slug == request.ProductCategorySlug, cancellationToken);
        
        if (category is null)
            throw new NotFoundException($"Категория не найдена {request.ProductCategorySlug}");
        
        product.ProductCategoryId = category.Id;
        product.Slug = slugService.GenerateSlug(request.Name!);
        
        if (request.Images != null && request.Images.Length > 0)
            product.Images = request.Images;
        
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}