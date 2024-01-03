namespace Domain.Entities;

public class Level : BaseEntity
{
    public List<Customer>? Customers { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
}