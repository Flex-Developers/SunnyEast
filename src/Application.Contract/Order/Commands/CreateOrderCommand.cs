namespace Application.Contract.Order.Commands;

public class CreateOrderCommand : IRequest<string>
{
    public required string ShopOrderSlug { get; set; }
    public int Quantity { get; set; } = 1;
    public required string CartSlug { get; set; }
}