using Application.Contract.Order.Commands;
using Application.Contract.Order.Queries;
using Application.Contract.Order.Responses;
using Application.Contract.Shops.Responses;
using Application.Contract.User.Responses;
using AutoMapper;

namespace Application.Features.Orders;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<CreateOrderCommand, Domain.Entities.Order>();
        
        CreateMap<Domain.Entities.Order, OrderResponse>()
            .ForMember(d => d.Customer,
                opt => opt.MapFrom(s => s.Customer))
            .ForMember(d => d.Sum,
                opt => opt.MapFrom(s => s.OrderItems!.Sum(i => i.SummaryPrice)))
            .ForMember(d => d.Customer,
                opt => opt.MapFrom(s => s.Customer))
            .ForMember(d => d.Shop,
                m => m.MapFrom(s => s.Shop))
            .ForMember(d => d.IsInArchive, m => m.MapFrom(s => s.IsInArchive));


        CreateMap<GetOrderQuery, OrderResponse>();

        CreateMap<Domain.Entities.OrderItem, OrderItemResponse>()
            .ForMember(d => d.UnitPrice,
                opt => opt.MapFrom(s => s.SummaryPrice / s.Quantity))
            .ForMember(d => d.ProductName,
                opt => opt.MapFrom(s => s.Product!.Name))
            .ForMember(d => d.Volume,
                opt => opt.MapFrom(s => s.Volume))
            .ForMember(d => d.ImageUrl,
                opt => opt.MapFrom(s =>
                    (s.Product!.Images != null && s.Product.Images.Length > 0) ? s.Product.Images.First() : string.Empty));

        CreateMap<Domain.Entities.Shop, ShopResponse>();
    }
}