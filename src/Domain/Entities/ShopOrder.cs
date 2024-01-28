namespace Domain.Entities;

public sealed class ShopOrder : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public required string ProductSlug { get; set; }

    public Guid ShopId { get; set; }
    public Shop? Shop { get; set; }
    public required string ShopSlug { get; set; }

    public int Quantity { get; set; }
}