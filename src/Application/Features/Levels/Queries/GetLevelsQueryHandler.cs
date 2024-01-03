using Application.Common.Interfaces.Contexts;
using Application.Contract.Level.Queries;
using Application.Contract.Level.Responses;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Levels.Queries;

public class GetLevelsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetLevelsQuery, List<LevelResponse>>
{
    public async Task<List<LevelResponse>> Handle(GetLevelsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Levels.Where(s => true);
        if (request.Slug != null) query = query.Where(s => s.Slug.Contains(request.Slug));

        if (request.Name != null) query = query.Where(s => s.Name.Contains(request.Name));


        return (await query.ToArrayAsync(cancellationToken))
            .Select(mapper.Map<LevelResponse>).ToList();
    }
}