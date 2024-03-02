namespace Application.Contract.ProductCategory.Responses;

public class ProductCategoryResponse
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public string? BaseCategorySlug { get; set; }
}