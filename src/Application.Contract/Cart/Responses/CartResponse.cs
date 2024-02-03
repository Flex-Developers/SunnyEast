using Application.Contract.Enums;

namespace Application.Contract.Cart.Responses;

public class CartResponse
{
    public required string Slug { get; set; }
    public required string ShopSlug { get; set; }
    public OrderStatus OrderStatus { get; set; }
}