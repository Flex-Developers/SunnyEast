using Application.Contract.Order.Responses;

namespace Application.Contract.Order.Commands;

public class CreateOrderCommand : IRequest<CreateOrderResponse>
{
    public required string ShopSlug { get; set; }
    public List<CreateOrderItem> Items { get; set; } = new();
    public string? Comment { get; set; }
}
