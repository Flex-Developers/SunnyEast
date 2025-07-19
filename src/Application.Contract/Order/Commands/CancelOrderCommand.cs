namespace Application.Contract.Order.Commands;

public class CancelOrderCommand : IRequest<Unit>
{
    public string Slug  { get; set; } = default!;
}