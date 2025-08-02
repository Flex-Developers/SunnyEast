using Application.Contract.Staff.Responses;
using AutoMapper;

namespace Application.Features.Staff;

public sealed class StaffProfile : Profile
{
    public StaffProfile()
    {
        CreateMap<Domain.Entities.Staff, StaffResponse>()
            .ForMember(d => d.Role,        o => o.MapFrom(s => s.StaffRole))
            .ForMember(d => d.UserName,    o => o.MapFrom(s => s.User.UserName))
            .ForMember(d => d.Email,       o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.User.PhoneNumber))
            .ForMember(d => d.Name,        o => o.MapFrom(s => s.User.Name))
            .ForMember(d => d.Surname,     o => o.MapFrom(s => s.User.Surname))
            .ForMember(d => d.ShopSlug,    o => o.MapFrom(s => s.Shop != null ? s.Shop.Slug    : null))
            .ForMember(d => d.ShopAddress, o => o.MapFrom(s => s.Shop != null ? s.Shop.Address : null));
    }
}