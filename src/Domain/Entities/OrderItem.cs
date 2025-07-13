using Domain.Enums;

namespace Domain.Entities;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public required string OrderSlug { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public required string ProductSlug { get; set; }

    public int Quantity { get; set; }
    public decimal SummaryPrice { get; set; }

    public OrderStatus Status { get; set; }
}
