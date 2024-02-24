namespace Application.Contract.Order.Responses;

public class OrderResponse
{
    public required string Slug { get; set; }
    public required string ShopOrderSlug { get; set; }
    public required string CartSlug { get; set; }
    public int Quantity { get; set; } = 0;

}