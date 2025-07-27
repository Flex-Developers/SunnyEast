using Application.Contract.Order.Responses;

namespace Application.Contract.Order.Queries;

public sealed class GetOrdersByUserQuery : IRequest<List<OrderResponse>>
{
    public Guid UserId { get; set; }
}