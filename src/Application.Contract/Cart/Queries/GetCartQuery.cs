using Application.Contract.Cart.Responses;

namespace Application.Contract.Cart.Queries;

public class GetCartQuery : IRequest<CartResponse>
{
    public required string Slug { get; set; }
}