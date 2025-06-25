using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Product.Commands;
using Application.Contract.Product.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands;

public class UpdateProductCommandHandler(IApplicationDbContext context, ISlugService slugService, IPriceCalculatorService priceCalculator)
    : IRequestHandler<UpdateProductCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Slug == request.Slug, cancellationToken);

        if (product is null)
            throw new NotFoundException($"Продукт не найден {request.Slug}");

        var previousPrice = product.Price;
        var previousDiscountPercentage = product.DiscountPercentage;
        var previousCategorySlug = product.ProductCategorySlug;

        product.Name = request.Name ?? product.Name;
        product.Price = request.Price ?? product.Price;
        product.DiscountPercentage = request.DiscountPercentage;
        product.Description = request.Description ?? product.Description;
        product.Slug = slugService.GenerateSlug(product.Name);

        if (request.Images is { Length: > 0 })
            product.Images = request.Images;

        if (!string.IsNullOrWhiteSpace(request.ProductCategorySlug) && request.ProductCategorySlug != previousCategorySlug)
        {
            var newCategory = await context.ProductCategories
                .FirstOrDefaultAsync(c => c.Slug == request.ProductCategorySlug, cancellationToken);

            if (newCategory is null)
                throw new NotFoundException($"Категория не найдена {request.ProductCategorySlug}");

            product.ProductCategoryId = newCategory.Id;
            product.ProductCategorySlug = newCategory.Slug;
        }

        var priceChanged = product.Price != previousPrice;
        var discountChanged = product.DiscountPercentage != previousDiscountPercentage;
        var categoryChanged = product.ProductCategorySlug != previousCategorySlug;

        // ----------- пересчёт цен (только при необходимости) -----------
        if (priceChanged || discountChanged || categoryChanged)
        {
            var category = await context.ProductCategories
                .FirstAsync(c => c.Slug == product.ProductCategorySlug, cancellationToken);

            var volumes = category.ProductVolumes
                          ?? throw new ValidationException("У категории отсутствует список объёмов");

            var fullPricePairs = priceCalculator.GetPrices(volumes, product.Price, null).ToArray();

            var discountedPairs = product.DiscountPercentage is > 0 ? priceCalculator
                    .GetPrices(volumes, product.Price, product.DiscountPercentage)
                    .ToArray() : [];

            var volumePrices = new List<VolumePrice>();

            for (var i = 0; i < volumes.Length; i++)
            {
                var volume = volumes[i];
                var fullPrice = fullPricePairs[i].Price!.Value;
                var discountPrice = discountedPairs.Length > 0 ? discountedPairs[i].Price : null;

                volumePrices.Add(new VolumePrice(volume, fullPrice, discountPrice));
            }

            product.VolumePricesJson = JsonSerializer.Serialize(volumePrices);

            // «минимальный» (первый) объём – синхронизируем vitrine-поля
            product.DiscountPrice = volumePrices[0].Discount;
        }

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}