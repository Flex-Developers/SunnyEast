namespace Application.Contract.Order.Commands;

public class UpdateOrderCommand : IRequest<string>
{
    public required string Slug { get; set; }
    public int Quantity { get; set; } = 1;
}