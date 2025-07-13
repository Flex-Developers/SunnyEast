using Application.Contract.Order.Commands;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using Client.Infrastructure.Services.Cart.Models;
using Client.Infrastructure.Services.HttpClient;
using System.Linq;

namespace Client.Infrastructure.Services.Order;

public class OrderService(IHttpClientService httpClient) : IOrderService
{
    public async Task<string?> CreateAsync(string shopSlug, IEnumerable<CartItemDto> items)
    {
        var command = new CreateOrderCommand
        {
            ShopSlug = shopSlug,
            Items = items.Select(i => new CreateOrderItem
            {
                ProductSlug = i.ProductSlug,
                Quantity = i.Quantity,
                SelectedVolume = i.SelectedVolume
            }).ToList()
        };

        var result = await httpClient.PostAsJsonAsync<string>("/api/order", command);
        return result.Success ? result.Response : null;
    }

    public async Task<List<OrderResponse>> GetAsync(string shopSlug)
    {
        var url = string.IsNullOrEmpty(shopSlug)
            ? "/api/order/GetOrders"
            : $"/api/order/GetOrders?ShopSlug={shopSlug}";
        var res = await httpClient.GetFromJsonAsync<List<OrderResponse>>(url);
        return res.Success ? res.Response ?? [] : [];
    }

    public async Task<OrderResponse?> GetAsyncBySlug(string slug)
    {
        var res = await httpClient.GetFromJsonAsync<OrderResponse>($"/api/order/GetOrder?Slug={slug}");
        return res.Success ? res.Response : null;
    }
}
