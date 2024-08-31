using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Product.Commands;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands;

public class CreateProductCommandHandler(IApplicationDbContext context, IMapper mapper, ISlugService slugService)
    : IRequestHandler<CreateProductCommand, string>
{
    public async Task<string> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = mapper.Map<Product>(request);

        product.Slug = slugService.GenerateSlug(product.Name);
        
        if (await context.Products.AnyAsync(s => s.Slug == product.Slug, cancellationToken))
            throw new ExistException($"Продукт с названием {product.Name} уже существует!");

        var category = await context.ProductCategories
            .FirstOrDefaultAsync(s => s.Slug == request.ProductCategorySlug, cancellationToken);
        
        if (category is null)
            throw new NotFoundException($"Категория не найдена {request.ProductCategorySlug}");
        
        product.ProductCategoryId = category.Id;
        
        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return product.Slug;
    }
}