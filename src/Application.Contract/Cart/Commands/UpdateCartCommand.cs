namespace Application.Contract.Cart.Commands;

public class UpdateCartCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
    public Guid ShopId { get; set; }
}