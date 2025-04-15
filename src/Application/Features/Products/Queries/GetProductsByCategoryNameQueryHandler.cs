using Application.Common.Interfaces.Contexts;
using Application.Contract.Product.Queries;
using Application.Contract.Product.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries;

public class GetProductsByCategoryNameQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetProductsByCategoryNameQuery, List<ProductResponse>>
{
    public async Task<List<ProductResponse>> Handle(GetProductsByCategoryNameQuery request, CancellationToken cancellationToken)
    {
        var products = await context.Products
            .Where(p => p.ProductCategory!.Name == request.CategoryName)
            .ToListAsync(cancellationToken);
        
        var result = products.Select(mapper.Map<ProductResponse>).ToList();
        
        var category = await context.ProductCategories
            .FirstOrDefaultAsync(c => c.Name == request.CategoryName, cancellationToken);

        if (category is not null)
        {
            foreach (var product in result)
            {
                product.ProductVolumes = category.ProductVolumes;
            }
        }

        return result;
    }
}
