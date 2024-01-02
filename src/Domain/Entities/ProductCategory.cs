namespace Domain.Entities;

public class ProductCategory : BaseEntity
{
    public required string Name { get; set; }

    public Guid? BaseProductCategoryId { get; set; }
    public ProductCategory? BaseProductCategory { get; set; }
}