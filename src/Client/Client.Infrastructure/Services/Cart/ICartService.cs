using Client.Infrastructure.Services.Cart.Models;

namespace Client.Infrastructure.Services.Cart;

public interface ICartService
{
    Task<CartDto> CreateOrderAsync(CartItemDto order);
    Task<CartDto?> GetCartAsync();
    Task<CartDto> UpdateOrderAsync(CartItemDto order);
    Task RemoveOrderAsync(string productSlug, string? selectedVolume);
    Task ClearAsync();
    Task<int> GetOrdersCountAsync();
    Task<bool> HasOrderAsync(string productSlug, string? selectedVolume);
    event Action? OnChange;
}