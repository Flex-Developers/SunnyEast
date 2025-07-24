using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Product.Responses;
using Application.Contract.ProductCategory.Commands;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductCategory.Commands;

public class UpdateProductCategoryCommandHandler(
    IApplicationDbContext context,
    ISlugService slugService,
    IMapper mapper,
    IPriceCalculatorService priceCalculator,
    IVolumeGroupService volumeGroupService)
    : IRequestHandler<UpdateProductCategoryCommand, string>
{
    public async Task<string> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        if (!volumeGroupService.AreFromSameGroup(request.ProductVolumes, out _, out var error)) 
            throw new ValidationException(error ?? "Некорректные объёмы категории.");
        
        var oldCategory = await context.ProductCategories
            .FirstOrDefaultAsync(s => s.Slug == request.Slug, cancellationToken);

        if (oldCategory is null)
            throw new NotFoundException($"Категория не найдена {request.Slug}");

        oldCategory.Name = request.Name.Trim();
        oldCategory.Slug = slugService.GenerateSlug(request.Name);
        var oldDiscount = oldCategory.DiscountPercentage;

        mapper.Map(request, oldCategory);

        await ApplyDiscountAsync(request, oldCategory, oldDiscount, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return oldCategory.Slug;
    }

    private async Task ApplyDiscountAsync(UpdateProductCategoryCommand request,
        Domain.Entities.ProductCategory category, byte? oldDiscount, CancellationToken ct)
    {
        if (request.DiscountPercentage is < 1 || request.DiscountPercentage == oldDiscount ||
            !request.ApplyDiscountToAllProducts)
            return;

        var products = await context.Products
            .Where(p => p.ProductCategoryId == category.Id)
            .ToListAsync(ct);

        foreach (var product in products)
        {
            product.DiscountPercentage = request.DiscountPercentage;
            UpdateDiscountPrice(product);

            var volumes = category.ProductVolumes ?? [];

            var vp = priceCalculator
                .GetPrices(volumes, product.Price, product.DiscountPercentage)
                .Select(t => new VolumePrice(t.Volume, t.Price!.Value, null));

            product.VolumePricesJson = JsonSerializer.Serialize(vp);
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