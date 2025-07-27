using Application.Contract.User.Commands;
using Application.Contract.User.Responses;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Users;

public class CustomerMapperProfile : Profile
{
    public CustomerMapperProfile()
    {
        CreateMap<RegisterUserCommand, ApplicationUser>();
        CreateMap<ApplicationUser, CustomerResponse>()
            .ForMember(d => d.Id,      o => o.MapFrom(s => s.Id))
            .ForMember(d => d.UserName,o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.Email,   o => o.MapFrom(s => s.Email))
            .ForMember(d => d.Phone,   o => o.MapFrom(s => s.PhoneNumber))
            .ForMember(d => d.Name,    o => o.MapFrom(s => s.Name))
            .ForMember(d => d.Surname, o => o.MapFrom(s => s.Surname))
            .ForMember(d => d.RegisteredAt, o => o.MapFrom(s => (DateTime?)s.CreatedAt))
            .ForMember(d => d.IsStaff,      o => o.Ignore()); 
    }
}