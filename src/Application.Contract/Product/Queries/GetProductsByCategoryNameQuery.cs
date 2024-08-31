using Application.Contract.Product.Responses;

namespace Application.Contract.Product.Queries;

public class GetProductsByCategoryNameQuery : IRequest<List<ProductResponse>>
{
    public string CategoryName { get; set; }
}