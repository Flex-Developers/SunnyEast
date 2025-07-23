using Application.Contract.SiteSettings.Commands;
using Application.Contract.SiteSettings.Responses;
using Client.Infrastructure.Services.HttpClient;

namespace Client.Infrastructure.Services.Settings;

public sealed class SettingsService(IHttpClientService http) : ISettingsService
{
    private const string BaseUrl = "/api/SiteSetting";

    public async Task<SiteSettingResponse?> GetAsync(CancellationToken ct = default)
    {
        var res = await http.GetFromJsonAsync<SiteSettingResponse>(BaseUrl);
        return res.Success ? res.Response : null;
    }

    public async Task<bool> UpdateAsync(UpdateSiteSettingCommand command, CancellationToken ct = default)
    {
        var res = await http.PutAsJsonAsync(BaseUrl, command);
        return res.Success;
    }
}
