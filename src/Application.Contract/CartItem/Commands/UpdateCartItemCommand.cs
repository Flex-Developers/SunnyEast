namespace Application.Contract.CartItem.Commands;

public class UpdateCartItemCommand : IRequest<string>
{
    public required string Slug { get; set; }
    public int Quantity { get; set; } = 1;
}