using Application.Contract.Shops.Responses;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.Shop;

public class ShopService(IHttpClientService httpClient) : IShopService
{
    public async Task<List<ShopResponse>> GetShopsAsync()
    {
        var res = await httpClient.GetFromJsonAsync<List<ShopResponse>>("/api/shop/GetShops");
        return res.Success ? res.Response ?? [] : [];
    }
}
