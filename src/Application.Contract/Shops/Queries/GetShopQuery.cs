using Application.Contract.Shops.Responses;

namespace Application.Contract.Shops.Queries;

public class GetShopQuery : IRequest<List<ShopResponse>>
{
    public string? Address { get; set; }
}