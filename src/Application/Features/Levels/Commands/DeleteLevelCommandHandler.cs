using Application.Common.Exceptions;
using Application.Common.Interfaces.Contexts;
using Application.Contract.Level.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Levels.Commands;

public class DeleteLevelCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteLevelCommand, Unit>
{
    public async Task<Unit> Handle(DeleteLevelCommand request, CancellationToken cancellationToken)
    {
        var level = await context.Levels.FirstOrDefaultAsync(s => s.Slug == request.Slug,
            cancellationToken);

        if (level != null)
        {
            context.Levels.Remove(level);
            await context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        throw new NotFoundException($"level with slug {request.Slug} not found");
    }
}