namespace Application.Contract.ProductCategory.Commands;

public class CreateProductCategoryCommand : IRequest<string>
{
    public required string Name { get; set; }
    public string? BaseCategorySlug { get; set; }
    public byte? DiscountPercentage { get; set; }
    public string? ImageUrl { get; set; }
}