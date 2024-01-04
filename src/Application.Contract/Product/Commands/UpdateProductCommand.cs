namespace Application.Contract.Product.Commands;

public class UpdateProductCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
    public string? Name { get; set; }
    public string? ProductCategorySlug { get; set; }
    public decimal? Price { get; set; }
}