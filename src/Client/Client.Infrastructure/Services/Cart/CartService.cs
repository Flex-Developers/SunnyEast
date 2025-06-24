using Blazored.LocalStorage;
using Client.Infrastructure.Services.Cart.Models;

namespace Client.Infrastructure.Services.Cart;

public class CartService(ILocalStorageService localStorageService) : ICartService
{
    private const string CartKey = "cart";
    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();

    public async Task<CartDto> CreateOrderAsync(CartItemDto order)
    {
        var cart = await GetCartInternalAsync();
        var existing = cart.Orders.FirstOrDefault(o => o.ProductSlug == order.ProductSlug && o.SelectedVolume == order.SelectedVolume);
        if (existing is not null)
        {
            existing.Quantity += order.Quantity;
        }
        else
        {
            cart.Orders.Add(order);
        }

        await SaveCartAsync(cart);
        NotifyStateChanged();
        return cart;
    }

    public async Task<CartDto?> GetCartAsync() => await localStorageService.GetItemAsync<CartDto>(CartKey);

    public async Task<CartDto> UpdateOrderAsync(CartItemDto order)
    {
        var cart = await GetCartInternalAsync();
        var existing = cart.Orders.FirstOrDefault(o => o.ProductSlug == order.ProductSlug && o.SelectedVolume == order.SelectedVolume);
        if (existing is not null)
        {
            existing.Quantity = order.Quantity;
            existing.SelectedVolume = order.SelectedVolume;
        }
        else
        {
            cart.Orders.Add(order);
        }

        await SaveCartAsync(cart);
        NotifyStateChanged();
        return cart;
    }

    public async Task RemoveOrderAsync(string productSlug, string? selectedVolume)
    {
        var cart = await GetCartInternalAsync();
        cart.Orders.RemoveAll(o => o.ProductSlug == productSlug && o.SelectedVolume == selectedVolume);
        await SaveCartAsync(cart);
        NotifyStateChanged();
    }

    public async Task ClearAsync()
    {
        await localStorageService.RemoveItemAsync(CartKey);
        NotifyStateChanged();
    }

    public async Task<int> GetOrdersCountAsync()
    {
        var cart = await GetCartAsync();
        return cart?.Orders.Count ?? 0;
    }

    public async Task<bool> HasOrderAsync(string productSlug, string? selectedVolume)
    {
        var cart = await GetCartAsync();
        return cart?.Orders.Any(o => o.ProductSlug == productSlug && o.SelectedVolume == selectedVolume) ?? false;
    }

    private async Task<CartDto> GetCartInternalAsync()
    {
        var cart = await localStorageService.GetItemAsync<CartDto>(CartKey);
        if (cart is null)
        {
            cart = new CartDto();
        }
        return cart;
    }

    private async Task SaveCartAsync(CartDto cart)
    {
        await localStorageService.SetItemAsync(CartKey, cart);
    }
}