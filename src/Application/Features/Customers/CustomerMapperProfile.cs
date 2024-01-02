using Application.Contract.Customer.Commands;
using Application.Contract.Customer.Responses;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Customers;

public class CustomerMapperProfile : Profile
{
    public CustomerMapperProfile()
    {
        CreateMap<CreateCustomerCommand, Customer>();
        CreateMap<Customer, CustomerResponse>();
    }
}