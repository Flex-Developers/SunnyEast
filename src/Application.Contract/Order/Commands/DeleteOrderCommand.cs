namespace Application.Contract.Order.Commands;

public class DeleteOrderCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
}