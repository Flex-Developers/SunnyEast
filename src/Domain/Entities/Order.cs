using Domain.Enums;

namespace Domain.Entities;

public sealed class Order : BaseEntity
{
    public required string Slug { get; set; }
    public List<OrderItem>? OrderItems { get; set; }

    public Guid ShopId { get; set; }
    public Shop? Shop { get; set; }
    public string? ShopSlug { get; set; }

    public Guid CustomerId { get; set; }
    public ApplicationUser? Customer { get; set; }

    public OrderStatus Status { get; set; }
    public string OrderNumber { get; set; } = default!;
    
    public string? CustomerComment { get; set; } // комментарий от клиента
    public DateTime? CanceledAt { get; set; } // если отменён
    public DateTime? ClosedAt { get; set; } // если обслужен
    public DateTime? CreatedAt { get; set; }
}
