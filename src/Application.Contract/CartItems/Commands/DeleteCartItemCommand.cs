namespace Application.Contract.Order.Commands;

public class DeleteCartItemCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
}