using Application.Contract.Product.Commands;
using Application.Contract.Product.Responses;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Products;

public class ProductMapperProfile : Profile
{
    public ProductMapperProfile()
    {
        CreateMap<CreateProductCommand, Product>();
        CreateMap<Product, ProductResponse>();
    }
}