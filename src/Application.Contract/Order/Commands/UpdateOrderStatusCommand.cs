namespace Application.Contract.Order.Commands;

using Application.Contract.Enums;

public class UpdateOrderStatusCommand : IRequest<Unit>
{
    public required string Slug { get; set; }
    public OrderStatus Status { get; set; }
}
