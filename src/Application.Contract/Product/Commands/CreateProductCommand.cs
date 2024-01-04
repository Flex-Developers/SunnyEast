namespace Application.Contract.Product.Commands;

public class CreateProductCommand : IRequest<string>
{
    public required string Name { get; set; }
    public required string ProductCategorySlug { get; set; }
    public required decimal Price { get; set; }
    public string? Image { get; set; }
}