using Application.Common.Interfaces.Contexts;
using Application.Contract.Product.Queries;
using Application.Contract.Product.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries;

public class GetProductsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetProductsQuery, List<ProductResponse>>
{
    public async Task<List<ProductResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Products.AsQueryable();

        if (request.Slug != null)
            query = query.Where(s => s.Slug == request.Slug);

        if (request.Name != null)
            query = query.Where(s => s.Name.Contains(request.Name));

        if (request.ProductCategorySlug != null)
            query = query.Where(s => s.ProductCategorySlug == request.ProductCategorySlug);
        
        var result = (await query.ToArrayAsync(cancellationToken))
            .Select(mapper.Map<ProductResponse>)
            .ToList();
        
        var categories = await context.ProductCategories
            .ToDictionaryAsync(c => c.Slug, cancellationToken);
        
        foreach (var product in result)
        {
            if (categories.TryGetValue(product.ProductCategorySlug, out var category))
            {
                product.ProductVolumes = category.ProductVolumes;
                product.SelectedVolume = product.ProductVolumes![0];
            }
        }

        return result;
    }
}