using Application.Contract.ProductCategory.Responses;

namespace Application.Contract.ProductCategory.Queries;

public class GetProductCategoryQuery : IRequest<ProductCategoryResponse>
{
    public required string Name { get; set; }
}