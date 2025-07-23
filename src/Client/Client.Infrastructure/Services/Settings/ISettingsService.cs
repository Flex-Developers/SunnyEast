using Application.Contract.SiteSettings.Commands;
using Application.Contract.SiteSettings.Responses;

namespace Client.Infrastructure.Services.Settings;

public interface ISettingsService
{
    Task<SiteSettingResponse?> GetAsync(CancellationToken ct = default);
    Task<bool> UpdateAsync(UpdateSiteSettingCommand command, CancellationToken ct = default);
}
