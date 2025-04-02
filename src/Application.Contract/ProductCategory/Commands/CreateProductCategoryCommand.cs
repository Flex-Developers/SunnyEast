namespace Application.Contract.ProductCategory.Commands;

public class CreateProductCategoryCommand : IRequest<string>
{
    public string? BaseCategorySlug { get; set; }
    public required string Name { get; set; }
    public byte? DiscountPercentage { get; set; }
    public string? ImageUrl { get; set; }
    public string[]? ProductVolumes { get; set; }
}