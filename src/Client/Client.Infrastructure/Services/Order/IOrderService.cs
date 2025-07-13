using Application.Contract.Order.Commands;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using Client.Infrastructure.Services.Cart.Models;

namespace Client.Infrastructure.Services.Order;

public interface IOrderService
{
    Task<string?> CreateAsync(string shopSlug, IEnumerable<CartItemDto> items);
    Task<List<OrderResponse>> GetAsync(string shopSlug);
    Task<OrderResponse?> GetAsyncBySlug(string slug);
}
