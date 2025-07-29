using Microsoft.AspNetCore.Components.Forms;

namespace Client.Infrastructure.Services.Database;

public interface IDatabaseAdminService
{
    Task<bool> DownloadAsync();                // скачать .sql.gz
    Task<bool> RestoreAsync(IBrowserFile file); // загрузить .sql или .sql.gz
}