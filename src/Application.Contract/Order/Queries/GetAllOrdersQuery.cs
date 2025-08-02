using Application.Contract.Order.Responses;

namespace Application.Contract.Order.Queries;

public class GetAllOrdersQuery : IRequest<List<OrderResponse>>
{
    public string? ShopSlug { get; set; }
    public bool OnlyArchived { get; set; }
}
