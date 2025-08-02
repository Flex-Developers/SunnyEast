using Application.Contract.Shops.Commands;
using Application.Contract.Shops.Responses;

namespace Client.Infrastructure.Services.Shop;

public interface IShopService
{
    Task<List<ShopResponse>> GetShopsAsync(CancellationToken cancellationToken = default);
    Task<List<ShopVm>> GetShopsVmAsync(CancellationToken cancellationToken = default);
    Task<bool> CreateAsync(CreateShopCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateShopCommand command, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string slug, CancellationToken cancellationToken = default);
}