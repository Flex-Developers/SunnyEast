using Application.Contract;
using Application.Contract.Product.Commands;
using Application.Contract.Product.Responses;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Products;

public class ProductMapperProfile : Profile
{
    public ProductMapperProfile()
    {
        CreateMap<CreateProductCommand, Product>()
            .ForMember(f => f.ProductPrice, opt => opt.MapFrom(s => s.ProductPrice));

        CreateMap<Product, ProductResponse>()
            .ForMember(f => f.ProductPrice, opt => opt.MapFrom(s => s.ProductPrice));

        CreateMap<ProductPrice, ProductPriceDto>();
        CreateMap<ProductPriceDto, ProductPrice>();
    }
}