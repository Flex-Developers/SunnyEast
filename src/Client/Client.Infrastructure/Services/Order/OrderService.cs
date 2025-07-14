using Application.Contract.Order.Commands;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using Application.Contract.Enums;
using Client.Infrastructure.Services.Cart.Models;
using Client.Infrastructure.Services.HttpClient;
using MudBlazor;

namespace Client.Infrastructure.Services.Order;

public class OrderService(IHttpClientService httpClient, ISnackbar snackbar) : IOrderService
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

        var res = await httpClient.PostAsJsonAsync<CreateOrderResponse>("/api/order", command);

        if (!res.Success)
        {
            snackbar.Add(httpClient.ExceptionMessage ?? "Ошибка оформления заказа.", Severity.Error);
            return null;
        }

        return res.Response?.Slug;
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

    public async Task UpdateStatusAsync(string slug, OrderStatus status)
    {
        var command = new UpdateOrderStatusCommand { Slug = slug, Status = status };
        await httpClient.PutAsJsonAsync("/api/order", command);
    }
}