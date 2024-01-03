using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Contract.Level.Commands;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Levels.Commands;

public class CreateLevelCommandHandler(IApplicationDbContext context, IMapper mapper, ISlugService slugService)
    : IRequestHandler<CreateLevelCommand, string>
{
    public async Task<string> Handle(CreateLevelCommand request, CancellationToken cancellationToken)
    {
        var level = mapper.Map<Level>(request);
        level.Slug = slugService.GenerateSlug(level.Name);
        if (await context.Levels.AnyAsync(s => s.Slug == level.Slug, cancellationToken))
            throw new ExistException($"Level with name {level.Name} and slug {level.Slug} already exist");

        await context.Levels.AddAsync(level, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return level.Slug;
    }
}