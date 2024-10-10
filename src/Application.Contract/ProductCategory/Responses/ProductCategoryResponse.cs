namespace Application.Contract.ProductCategory.Responses;

public class ProductCategoryResponse
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public byte? DiscountPercentage { get; set; }
    public string? BaseCategorySlug { get; set; }
    public string? ImageUrl { get; set; }
}