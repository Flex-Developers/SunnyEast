using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Product.Commands;
using Application.Contract.Product.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands;

public class CreateProductCommandHandler(IApplicationDbContext context, IMapper mapper, ISlugService slugService, IPriceCalculatorService priceCalculator)
    : IRequestHandler<CreateProductCommand, string>
{
    public async Task<string> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = mapper.Map<Product>(request);
        product.Slug = slugService.GenerateSlug(product.Name);

        if (await context.Products.AnyAsync(p => p.Slug == product.Slug, cancellationToken))
            throw new ExistException($"Продукт с названием {product.Name} уже существует!");

        var category = await context.ProductCategories
            .FirstOrDefaultAsync(c => c.Slug == request.ProductCategorySlug, cancellationToken);

        if (category is null)
            throw new NotFoundException($"Категория не найдена {request.ProductCategorySlug}");

        product.ProductCategoryId = category.Id;

        // ---------- формируем список цен ----------
        var volumes = category.ProductVolumes;
        
        if (volumes is null || volumes.Length == 0)
            throw new ValidationException("У категории отсутствует список объёмов");

        var fullPricePairs = priceCalculator.GetPrices(volumes, request.Price!.Value, null).ToArray();

        var discountedPairs = request.DiscountPercentage is > 0 ? priceCalculator
                .GetPrices(volumes, request.Price.Value, request.DiscountPercentage)
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

        // «минимальный» (первый) объём – это поле Price / DiscountPrice
        product.Price = (decimal) volumePrices[0].Full!;
        product.DiscountPrice = volumePrices[0].Discount;

        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return product.Slug;
    }
}