using Application.Contract.Shops.Commands;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Shops.Queries;

public class CreateShopProfile : Profile
{
    public CreateShopProfile()
    {
        CreateMap<CreateShopCommand, Shop>();
    }
}