using Application.Contract.Cart.Commands;
using Application.Contract.Cart.Queries;
using Application.Contract.Cart.Responses;

namespace Client.Infrastructure.Services.Cart;

public interface ICartService
{
    public Task<bool> PostAsync(CreateCartCommand command);
    public Task<bool> PutAsync(UpdateCartCommand command);
    public Task<bool> DeleteAsync(Guid id);
    public Task<CartResponse> GetAsync(GetCartQuery query);
    public Task<List<CartResponse>> GetAllAsync(GetCartsQuery query);
}