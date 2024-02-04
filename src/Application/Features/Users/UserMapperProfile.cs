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
        CreateMap<ApplicationUser, CustomerResponse>();
    }
}