using Application.Contract.Shops.Commands;
using Application.Contract.Shops.Queries;
using Application.Contract.Shops.Responses;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Shops;

public class ShopProfile : Profile
{
    public ShopProfile()
    {
        CreateMap<CreateShopCommand, Shop>();
        CreateMap<Shop, ShopResponse>();
    }
}