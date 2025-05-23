namespace Application.Contract.ProductCategory.Commands;

public class UpdateProductCategoryCommand : IRequest<string>
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public string? BaseCategorySlug { get; set; }
    public byte? DiscountPercentage { get; set; }
    public bool ApplyDiscountToAllProducts { get; set; }
    public string? ImageUrl { get; set; }
    public string[]? ProductVolumes { get; set; }
}