using Application.Common.Interfaces.Contexts;
using Application.Contract.ProductCategory.Queries;
using Application.Contract.ProductCategory.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductCategory.Queries;

public class GetProductCategoriesQueryHandler : IRequestHandler<GetProductCategoriesQuery,
    List<ProductCategoryResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ProductCategoryResponse>> Handle(GetProductCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var productCategoriesResponse = await _context.ProductCategories
            .Select(s => _mapper.Map<ProductCategoryResponse>(s))
            .ToListAsync(cancellationToken);
        return productCategoriesResponse;
    }
}