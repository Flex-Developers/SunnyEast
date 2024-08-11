using Application.Contract.ProductCategory.Commands;
using Application.Contract.ProductCategory.Responses;
using AutoMapper;

namespace Application.Features.ProductCategory;

public class ProductCategoryProfile : Profile
{
    public ProductCategoryProfile()
    {
        CreateMap<CreateProductCategoryCommand, Domain.Entities.ProductCategory>();
        CreateMap<Domain.Entities.ProductCategory, ProductCategoryResponse>();
        CreateMap<ProductCategoryResponse, Domain.Entities.ProductCategory>();
    }
}