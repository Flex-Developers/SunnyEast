using Application.Common.Interfaces.Services;
using Application.Contract.Database.Queries;
using Application.Contract.Database.Responses;
using MediatR;

namespace Application.Features.Database.Queries;


public sealed class DownloadDatabaseBackupQueryHandler(IDbSnapshotService snapshots) : IRequestHandler<DownloadDatabaseBackupQuery, DatabaseBackupResponse>
{
    public Task<DatabaseBackupResponse> Handle(DownloadDatabaseBackupQuery request, CancellationToken cancellationToken)
        => snapshots.CreateBackupAsync(cancellationToken);
}