using Application.Contract.Order.Responses;

namespace Application.Contract.Order.Queries;

public sealed class GetSalesmanOrdersQuery : IRequest<List<OrderResponse>>
{
    /// <summary>
    /// Необязательно. Если передан – фильтруем только по одному магазину.
    /// </summary>
    public string? ShopSlug { get; set; }

    public bool OnlyArchived { get; set; }
}