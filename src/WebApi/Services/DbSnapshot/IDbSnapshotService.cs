namespace WebApi.Services.DbSnapshot;

public interface IDbSnapshotService
{
    /// <summary>Создать полный бэкап БД и вернуть путь к временному .sql.gz файлу.</summary>
    Task<string> CreateBackupGzipAsync(CancellationToken ct = default, bool skipGate = false);

    /// <summary>Полностью восстановить БД из .sql или .sql.gz. Перед восстановлением делает автобэкап.</summary>
    Task RestoreFromDumpAsync(Stream uploadedFile, string? fileName, CancellationToken ct = default);
}