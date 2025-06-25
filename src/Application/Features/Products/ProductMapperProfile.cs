using System.Text.Json;
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

        CreateMap<Product, ProductResponse>()
            .ForMember(d => d.Prices,
                o => o.ConvertUsing(new VolumePriceConverter(), src => src.VolumePricesJson))
            .AfterMap((src, dest) =>
            {
                if (dest.Prices.Count > 0)
                    dest.SelectedVolume = dest.Prices[0].Volume;
            });
    }
}

public class VolumePriceConverter : IValueConverter<string?, IReadOnlyList<VolumePrice>>
{
    public IReadOnlyList<VolumePrice> Convert(string? sourceMember, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(sourceMember))
            return new List<VolumePrice>();

        return JsonSerializer.Deserialize<IReadOnlyList<VolumePrice>>(sourceMember!) ?? new List<VolumePrice>();
    }
}
