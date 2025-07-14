using Application.Contract.Enums;

namespace Application.Contract.Order.Responses;

public class OrderResponse
{
    public required string Slug { get; set; }
    public required string ShopSlug { get; set; }
    public string OrderNumber { get; set; } = default!;
    public OrderStatus Status { get; set; }
    public decimal Sum { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public DateTime? ClosedAt   { get; set; }
    public DateTime? CanceledAt { get; set; }
    public string?  CustomerComment { get; set; }
}