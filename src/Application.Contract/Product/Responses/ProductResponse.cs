namespace Application.Contract.Product.Responses;

public record ProductResponse
{
    public required string Slug { get; set; }
    public required string LevelSlug { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
}