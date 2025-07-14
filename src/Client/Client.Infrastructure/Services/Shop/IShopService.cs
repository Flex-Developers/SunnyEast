using Application.Contract.Shops.Queries;
using Application.Contract.Shops.Responses;

namespace Client.Infrastructure.Services.Shop;

public interface IShopService
{
    Task<List<ShopResponse>> GetShopsAsync();
}
