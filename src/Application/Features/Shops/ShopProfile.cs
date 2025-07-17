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
        CreateMap<CreateShopCommand, Shop>()
            .ForMember(d => d.Images,
                c => c.MapFrom(s => s.Images ?? Array.Empty<string>()));
        CreateMap<Shop, UpdateShopCommand>()
            .ForAllMembers(opt => opt.Condition(
                (src, _, val) => val != null));
        
        CreateMap<UpdateShopCommand, Shop>();
        CreateMap<Shop, ShopResponse>();
    }
}