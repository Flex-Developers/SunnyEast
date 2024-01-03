using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Level.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Levels.Commands;

public class UpdateLevelCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateLevelCommand, Unit>
{
    public async Task<Unit> Handle(UpdateLevelCommand request, CancellationToken cancellationToken)
    {
        var level = await context.Levels.FirstOrDefaultAsync(c => c.Slug == request.Slug, cancellationToken);
        if (level == null) throw new NotFoundException($"Level with slug {request.Slug}");
        level.Name = request.Name;
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}