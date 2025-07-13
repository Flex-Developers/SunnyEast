namespace Application.Contract.Order.Commands;

public class CreateOrderCommand : IRequest<string>
{
    public required string ShopSlug { get; set; }
    public List<CreateOrderItem> Items { get; set; } = new();
}
