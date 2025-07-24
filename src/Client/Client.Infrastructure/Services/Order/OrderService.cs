using Application.Contract.Order.Commands;
using Application.Contract.Order.Responses;
using Application.Contract.Enums;
using Client.Infrastructure.Services.Cart.Models;
using Client.Infrastructure.Services.HttpClient;
using MudBlazor;

namespace Client.Infrastructure.Services.Order;

public class OrderService(IHttpClientService httpClient, ISnackbar snackbar) : IOrderService
{
    public async Task<CreateOrderResponse?> CreateAsync(string shopSlug, IEnumerable<CartItemDto> items, string? comment = null)
    {
        var command = new CreateOrderCommand
        {
            ShopSlug = shopSlug,
            Comment = comment,
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

        return res.Response;
    }


    public async Task<List<OrderResponse>> GetAsync(string shopSlug, bool archived = false)
    {
        var url = string.IsNullOrEmpty(shopSlug)
            ? $"/api/order/GetOrders?OnlyArchived={archived.ToString().ToLower()}"
            : $"/api/order/GetOrders?ShopSlug={shopSlug}&OnlyArchived={archived.ToString().ToLower()}";

        var res = await httpClient.GetFromJsonAsync<List<OrderResponse>>(url);
        return res.Success ? res.Response ?? [] : [];
    }

    public async Task<OrderResponse?> GetAsyncBySlug(string slug)
    {
        var res = await httpClient.GetFromJsonAsync<OrderResponse>($"/api/order/GetOrder?Slug={slug}");
        return res.Success ? res.Response : null;
    }

    public async Task<bool> UpdateStatusAsync(string slug, OrderStatus status)
    {
        var cmd = new ChangeOrderStatusCommand { Slug = slug, Status = status};
        return (await httpClient.PutAsJsonAsync("/api/order/change-status", cmd)).Success;
    }

    public async Task<bool> CancelOwnAsync(string slug)
    {
        var res = await httpClient.PutAsync($"/api/order/{Uri.EscapeDataString(slug)}/cancel");

        return res.Success;
    }

    public async Task<bool> ArchiveAsync(string slug, OrderStatus status)
    {
        var cmd = new ArchiveOrderCommand
        {
            Slug = slug,
            CurrentStatus = status,
        };
        return (await httpClient.PutAsJsonAsync($"/api/order/{slug}/archive", cmd)).Success;
    }
}