using Client.Infrastructure.Preferences;
using Client.Infrastructure.Theme;
using MudBlazor;

namespace Client.Layout;

public partial class MainLayout
{
    private ClientPreference? _themePreference;
    private readonly MudTheme _currentTheme = ApplicationThemes.DefaultTheme;
    private bool _rightToLeft;
    private bool _isDarkMode;
    private MudThemeProvider _mudThemeProvider = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            _isDarkMode = await _mudThemeProvider.GetSystemPreference();
    }

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
        _isDarkMode = themePreference.IsDarkMode;
        StateHasChanged();
    }
}