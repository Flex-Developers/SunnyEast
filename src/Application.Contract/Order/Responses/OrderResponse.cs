using Application.Contract.Enums;
using Application.Contract.Shops.Responses;
using Application.Contract.User.Responses;

namespace Application.Contract.Order.Responses;

public class OrderResponse
{
    public required string Slug { get; set; }
    public ShopResponse? Shop { get; set; }
    public string OrderNumber { get; set; } = default!;
    public OrderStatus Status { get; set; }
    public decimal Sum { get; set; }
    public bool IsInArchive { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public CustomerResponse  Customer { get; set; } = default!;
    public DateTime? ClosedAt   { get; set; }
    public DateTime? CanceledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string?  CustomerComment { get; set; }
}