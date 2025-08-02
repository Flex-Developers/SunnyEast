using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Contract.Database.Commands;
using MediatR;

namespace Application.Features.Database.Commands;

public sealed class RestoreDatabaseCommandHandler(IDbSnapshotService snapshots) : IRequestHandler<RestoreDatabaseCommand, Unit>
{
    public async Task<Unit> Handle(RestoreDatabaseCommand request, CancellationToken cancellationToken)
    {
        if (request.FileStream is null)
            throw new BadRequestException("Файл не загружен.");

        await snapshots.RestoreAsync(request.FileStream, request.FileName, cancellationToken);
        return Unit.Value;
    }
}