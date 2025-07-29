using Application.Contract.Database.Responses;

namespace Application.Contract.Database.Queries;

public sealed record DownloadDatabaseBackupQuery : IRequest<DatabaseBackupResponse>;