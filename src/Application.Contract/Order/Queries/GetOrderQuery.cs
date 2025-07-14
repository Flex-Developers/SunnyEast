using Application.Contract.Order.Responses;

namespace Application.Contract.Order.Queries;

public class GetOrderQuery : IRequest<OrderResponse>
{
    public required string Slug { get; set; }
}
