namespace Application.Contract.Product.Responses;

public record VolumePrice(string Volume, decimal? Full, decimal? Discount);

public record ProductResponse
{
    public required string Slug { get; set; }
    public required string ProductCategorySlug { get; set; }
    public string Name { get; set; } = "";
    public required decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public byte? DiscountPercentage { get; set; }
    public string[]? Images { get; set; }
    public string? Description { get; set; }
    public string? SelectedVolume { get; set; }
    public string[]? ProductVolumes { get; set; }
    public int Quantity { get; set; } = 1;
    public required IReadOnlyList<VolumePrice> Prices { get; init; }

}