using System.ComponentModel.DataAnnotations;

namespace Application.Contract.Product.Commands;

public class CreateProductCommand : IRequest<string>
{
    public required string Name { get; set; }
    public required string ProductCategorySlug { get; set; }
    public required decimal? Price { get; set; }
    public string? Description { get; set; }
    public string?[] Images { get; set; }
}
