namespace Domain.Entities;

public class Product : BaseEntity
{
    public required string Name { get; set; }
    public Guid ProductCategoryId { get; set; }
    public ProductCategory? ProductCategory { get; set; }

    public float Discount { get; set; }
    public decimal Price { get; set; }
    public decimal OldPrice { get; set; }
    public string[]? Images { get; set; }
}