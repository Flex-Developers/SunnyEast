using Application.Contract.Enums;

namespace Application.Contract.Order.Responses;

public class OrderResponse
{
    public required string Slug { get; set; }
    public required string ShopSlug { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Sum { get; set; }
}
