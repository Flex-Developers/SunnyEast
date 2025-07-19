namespace Application.Contract.Order.Responses;

public class CreateOrderResponse
{
    public required string Slug { get; set; }
    public required string OrderNumber { get; set; }
}