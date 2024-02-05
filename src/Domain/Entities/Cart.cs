using Domain.Enums;

namespace Domain.Entities;

public sealed class Cart : BaseEntity
{
    public required string Slug { get; set; }
    public List<Order>? Orders { get; set; }

    public Guid ShopId { get; set; }
    public Shop? Shop { get; set; }
    public string? ShopSlug { get; set; }
    
    public Guid CustomerId { get; set; }
    public ApplicationUser? Customer { get; set; }

    public OrderStatus Status { get; set; }
}