namespace Application.Contract.CartItem.Commands;

public class DeleteCartItemCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
}