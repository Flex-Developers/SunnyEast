using Client.Infrastructure.Preferences;
using Client.Infrastructure.Theme;
using MudBlazor;

namespace Client.Layout;

public partial class MainLayout
{
    private ClientPreference? _themePreference;
    private MudTheme _currentTheme = new LightTheme();
    private bool _rightToLeft;

    protected override async Task OnInitializedAsync()
    {
        _themePreference = await ClientPreferences.GetPreference() as ClientPreference;

        if (_themePreference == null)
            _themePreference = new ClientPreference();

        SetCurrentTheme(_themePreference);
    }

    private async Task ThemePreferenceChanged(ClientPreference themePreference)
    {
        SetCurrentTheme(themePreference);
        await ClientPreferences.SetPreference(themePreference);
    }

    private void SetCurrentTheme(ClientPreference themePreference)
    {
        _currentTheme = themePreference.IsDarkMode ? new DarkTheme() : new LightTheme();
        _currentTheme.PaletteDark.Primary = themePreference.PrimaryColor;
        _currentTheme.PaletteDark.Secondary = themePreference.SecondaryColor;
        _currentTheme.LayoutProperties.DefaultBorderRadius = $"{themePreference.BorderRadius}px";
        _currentTheme.LayoutProperties.DefaultBorderRadius = $"{themePreference.BorderRadius}px";
        _rightToLeft = themePreference.IsRTL;
    }
}