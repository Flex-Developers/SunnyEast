using Application.Contract.Order.Responses;

namespace Application.Contract.Order.Queries;

public class GetOrdersQuery : IRequest<List<OrderResponse>>
{
    public string? ShopSlug { get; set; }
}
