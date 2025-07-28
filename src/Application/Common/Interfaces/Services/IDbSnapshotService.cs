using Application.Contract.Database.Responses;

namespace Application.Common.Interfaces.Services;

public interface IDbSnapshotService
{
    Task<DatabaseBackupResponse> CreateBackupAsync(CancellationToken ct);
    Task RestoreAsync(Stream fileStream, string fileName, CancellationToken ct);
}