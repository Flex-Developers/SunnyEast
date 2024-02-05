using Application.Contract.Cart.Responses;

namespace Application.Contract.Cart.Queries;

public class GetCartsQuery : IRequest<List<CartResponse>> //TODO: Complete this
{
    public string? ShopSlug { get; set; }
}