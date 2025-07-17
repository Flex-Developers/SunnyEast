using Application.Contract.Shops.Commands;
using Application.Contract.Shops.Responses;

namespace Client.Infrastructure.Services.Shop;

public interface IShopService
{
    Task<List<ShopResponse>> GetShopsAsync(CancellationToken ct = default);
    Task<bool> CreateAsync(CreateShopCommand command, CancellationToken ct = default);
    Task<bool> UpdateAsync(UpdateShopCommand command, CancellationToken ct = default);
    Task<bool> DeleteAsync(string slug, CancellationToken ct = default);
}