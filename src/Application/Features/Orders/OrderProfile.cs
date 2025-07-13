using Application.Contract.Order.Commands;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using AutoMapper;

namespace Application.Features.Orders;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<CreateOrderCommand, Domain.Entities.Order>();
        CreateMap<Domain.Entities.Order, OrderResponse>();
        CreateMap<GetOrderQuery, OrderResponse>();
    }
}
