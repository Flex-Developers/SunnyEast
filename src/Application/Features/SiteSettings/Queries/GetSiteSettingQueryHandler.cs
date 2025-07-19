using Application.Common.Interfaces.Contexts;
using Application.Contract.SiteSettings.Queries;
using Application.Contract.SiteSettings.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.SiteSettings.Queries;

public class GetSiteSettingQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetSiteSettingQuery, SiteSettingResponse>
{
    public async Task<SiteSettingResponse> Handle(GetSiteSettingQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.SiteSettings.FirstOrDefaultAsync(cancellationToken);
        return entity is null ? new SiteSettingResponse() : mapper.Map<SiteSettingResponse>(entity);
    }
}
