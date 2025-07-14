namespace Application.Contract.Order.Responses;

public class OrderItemResponse
{
    public required string ProductSlug { get; set; }
    public required string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SummaryPrice => UnitPrice * Quantity;
}