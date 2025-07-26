using Application.Contract.Shops.Commands;
using Application.Contract.Shops.Responses;
using Client.Infrastructure.Services.HttpClient;
using MudBlazor;

namespace Client.Infrastructure.Services.Shop;

public sealed class ShopService(IHttpClientService http, ISnackbar snackbar) : IShopService
{
    private const string Base = "/api/shop";

    public async Task<List<ShopResponse>> GetShopsAsync(CancellationToken cancellationToken = default)
    {
        var res = await http.GetFromJsonAsync<List<ShopResponse>>($"{Base}/GetShops");
        return res.Success ? res.Response ?? [] : [];
    }

    public async Task<List<ShopVm>> GetShopsVmAsync(CancellationToken cancellationToken = default)
    {
        var res = await http.GetFromJsonAsync<List<ShopVm>>($"{Base}/GetShopVm");
        return res.Success ? res.Response ?? [] : [];
    }

    public async Task<bool> CreateAsync(CreateShopCommand cmd, CancellationToken cancellationToken = default)
    {
        var res = await http.PostAsJsonAsync(Base, cmd);
        return res.Success;
    }

    public async Task<bool> UpdateAsync(UpdateShopCommand cmd, CancellationToken cancellationToken = default)
    {
        var res = await http.PutAsJsonAsync(Base, cmd);
        return res.Success;
    }

    public async Task<bool> DeleteAsync(string slug, CancellationToken cancellationToken = default)
    {
        var res = await http.DeleteAsync($"{Base}/{slug}");

        if (!res.Success)
            snackbar.Add(http.ExceptionMessage ?? "Не удалось удалить магазин.", Severity.Error);

        return res.Success;
    }
}