namespace Domain.Entities;

public class Product : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required Guid ProductCategoryId { get; set; }
    public required string ProductCategorySlug { get; set; }
    public ProductCategory? ProductCategory { get; set; }
    public string? Description { get; set; }

    public required decimal Price { get; set; }
    public string[]? Images { get; set; }
}