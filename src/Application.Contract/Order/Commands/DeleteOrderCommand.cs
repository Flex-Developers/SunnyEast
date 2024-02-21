namespace Application.Contract.Order.Commands;

public class DeleteOrderCommand : IRequest<string>
{
    public required string OrderSlug { get; set; }
}