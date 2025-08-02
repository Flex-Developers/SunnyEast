using Application.Contract.Order.Commands;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using Application.Contract.Enums;
using Client.Infrastructure.Services.Cart.Models;

namespace Client.Infrastructure.Services.Order;

public interface IOrderService
{
    Task<CreateOrderResponse?> CreateAsync(string shopSlug, IEnumerable<CartItemDto> items, string? comment = null);
    Task<List<OrderResponse>> GetAllAsync(string shopSlug, bool archived = false);
    Task<List<OrderResponse>> GetAllByUserAsync(Guid userId);
    Task<List<OrderResponse>> GetCustomerAsync();
    Task<List<OrderResponse>> GetSalesmanAsync(string? shopSlug = null, bool archived = false);
    Task<OrderResponse?> GetAsyncBySlug(string slug);
    Task<bool> UpdateStatusAsync(string slug, OrderStatus status);
    Task<bool> ArchiveAsync(string slug, OrderStatus status);
    Task<bool> CancelOwnAsync(string slug);
}
