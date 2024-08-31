using Application.Common.Interfaces.Contexts;
using Application.Contract.ProductCategory.Queries;
using Application.Contract.ProductCategory.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductCategory.Queries;

public class GetProductCategoryQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetProductCategoryQuery, ProductCategoryResponse>
{
    public async Task<ProductCategoryResponse> Handle(GetProductCategoryQuery request, CancellationToken cancellationToken)
    {
        var result =
            await context.ProductCategories.FirstOrDefaultAsync(c => c.Name == request.Name, cancellationToken);
        return mapper.Map<ProductCategoryResponse>(result);
    }
}