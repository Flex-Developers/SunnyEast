using Application.Contract.Order.Commands;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using Application.Contract.Enums;
using Client.Infrastructure.Services.Cart.Models;

namespace Client.Infrastructure.Services.Order;

public interface IOrderService
{
    Task<CreateOrderResponse?> CreateAsync(string shopSlug, IEnumerable<CartItemDto> items, string? comment = null);
    Task<List<OrderResponse>> GetAsync(string shopSlug, bool archived = false);
    Task<OrderResponse?> GetAsyncBySlug(string slug);
    Task UpdateStatusAsync(string slug, OrderStatus status, string? comment =  null);
    Task ArchiveAsync(string slug, bool value, OrderStatus status);
    Task CancelOwnAsync(string slug);
}
