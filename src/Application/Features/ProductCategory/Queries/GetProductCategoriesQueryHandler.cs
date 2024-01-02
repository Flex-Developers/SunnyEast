using Application.Common.Interfaces.Contexts;
using Application.Contract.ProductCategory.Queries;
using Application.Contract.ProductCategory.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductCategory.Queries;

public class GetProductCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetProductCategoriesQuery,
        List<ProductCategoryResponse>>
{
    public async Task<List<ProductCategoryResponse>> Handle(GetProductCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var productCategoriesResponse = await context.ProductCategories
            .Select(s => mapper.Map<ProductCategoryResponse>(s))
            .ToListAsync(cancellationToken);
        return productCategoriesResponse;
    }
}