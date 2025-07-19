using Application.Common.Interfaces.Contexts;
using Application.Contract.SiteSettings.Commands;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.SiteSettings.Commands;

public class UpdateSiteSettingCommandHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<UpdateSiteSettingCommand, Unit>
{
    public async Task<Unit> Handle(UpdateSiteSettingCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.SiteSettings.FirstOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            entity = new SiteSetting
            {
                PhoneNumber = string.Empty,
                Email = string.Empty,
                Address = string.Empty
            };
            await context.SiteSettings.AddAsync(entity, cancellationToken);
        }

        mapper.Map(request, entity);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
