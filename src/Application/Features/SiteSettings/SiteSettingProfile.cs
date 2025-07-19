using Application.Contract.SiteSettings.Commands;
using Application.Contract.SiteSettings.Responses;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.SiteSettings;

public class SiteSettingProfile : Profile
{
    public SiteSettingProfile()
    {
        CreateMap<SiteSetting, SiteSettingResponse>();
        CreateMap<UpdateSiteSettingCommand, SiteSetting>();
    }
}
