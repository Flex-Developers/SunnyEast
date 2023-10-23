using Application.Contract.ProductCategory.Responses;

namespace Application.Contract.ProductCategory.Queries;

public class GetProductCategoriesQuery : IRequest<List<ProductCategoryResponse>>
{
}