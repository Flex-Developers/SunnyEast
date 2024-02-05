using Application.Contract.Cart.Commands;
using Application.Contract.Cart.Queries;
using Application.Contract.Cart.Responses;
using AutoMapper;

namespace Application.Features.Cart;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<GetCartQuery, CartResponse>();
        CreateMap<Domain.Entities.Cart, CartResponse>(); // Cart props copies to CartResponse
        CreateMap<CartResponse, Domain.Entities.Cart>();
        CreateMap<CreateCartCommand, Domain.Entities.Cart>();
    }
}