using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.ProductCategory.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductCategory.Commands;

public class UpdateProductCategoryCommandHandler(IApplicationDbContext context, ISlugService slugService)
    : IRequestHandler<UpdateProductCategoryCommand, string>
{
    public async Task<string> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var old = await context.ProductCategories
            .FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);

        if (old is null)
            throw new NotFoundException($"Категория не найдена {request.Slug}");
        
        old.Name = request.Name.Trim();
        old.ImageUrl = request.ImageUrl;
        old.DiscountPercentage = request.DiscountPercentage;
        old.ApplyDiscountToAllProducts = request.ApplyDiscountToAllProducts;
        old.Slug = slugService.GenerateSlug(request.Name);

        await ApplyDiscount(request, old, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return old.Slug;
    }

    private async Task ApplyDiscount(UpdateProductCategoryCommand request, Domain.Entities.ProductCategory old, CancellationToken cancellationToken)
    {
        // If the discount percentage is less than 1 or already matches the old value, exit the method
        if (request.DiscountPercentage is < 1 || old.DiscountPercentage == request.DiscountPercentage)
            return;

        // Create an initial query to get products by category name
        IQueryable<Product> productsQuery = context.Products.Where(p => p.ProductCategory!.Name == request.Name);

        // If we do not want to apply discount to all products, add a condition to filter
        if (request.ApplyDiscountToAllProducts is false)
            productsQuery = productsQuery.Where(p => p.DiscountPercentage == null);

        // Execute the query and get the list of products
        List<Product> products = await productsQuery.ToListAsync(cancellationToken);

        // Update the discount percentage and calculate the new discounted price for each product
        foreach (var product in products)
        {
            product.DiscountPercentage = request.DiscountPercentage; // Set the new discount percentage
            UpdateDiscountPrice(product); // Call method to update the discounted price
        } 
    }

    private static void UpdateDiscountPrice(Product product)
    {
        if (product.Price > 0)
        {
            var discount = product.Price * (product.DiscountPercentage / 100m);
            product.DiscountPrice = Math.Round((decimal)(product.Price - discount)!, 2);
        }
    }
}