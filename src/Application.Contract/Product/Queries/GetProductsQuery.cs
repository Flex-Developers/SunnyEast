using Application.Contract.Product.Responses;

namespace Application.Contract.Product.Queries;

public class GetProductsQuery : IRequest<List<ProductResponse>>
{
    public string? Slug { get; set; }
    public string? ProductCategorySlug { get; set; }
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}