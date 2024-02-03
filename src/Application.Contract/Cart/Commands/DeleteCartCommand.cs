namespace Application.Contract.Cart.Commands;

public class DeleteCartCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
}