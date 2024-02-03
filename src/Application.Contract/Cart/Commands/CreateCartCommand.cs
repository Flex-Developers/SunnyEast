namespace Application.Contract.Cart.Commands;

public class CreateCartCommand : IRequest<string>
{
    public required string ShopSlug { get; set; } // this is a request
}