namespace Application.Contract.Product.Responses;

public record ProductResponse
{
    public required string Slug { get; set; }
    public required string ProductCategorySlug { get; set; }
    public required string LevelSlug { get; set; }
    public string Name { get; set; } = "";
    public byte? DiscountPercentage { get; set; }
    public string[]? Images { get; set; }
    public string? Description { get; set; }
    public ProductPriceDto? ProductPrice { get; set; }
}