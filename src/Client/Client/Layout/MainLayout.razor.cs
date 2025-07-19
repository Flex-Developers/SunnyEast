using Client.Infrastructure.Preferences;
using Client.Infrastructure.Theme;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Layout;

public partial class MainLayout
{
    private ClientPreference? _themePreference;
    private readonly MudTheme _currentTheme = ApplicationThemes.DefaultTheme;
    private bool _isDarkMode;
    private MudThemeProvider _mudThemeProvider = null!;

    [Parameter] public EventCallback OnDarkModeToggle { get; set; }
    private bool _drawerOpen;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isDarkMode = await _mudThemeProvider.GetSystemPreference();
            StateHasChanged();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _themePreference = await ClientPreferences.GetPreference() as ClientPreference;

        if (_themePreference == null)
            _themePreference = new ClientPreference();

        SetCurrentTheme(_themePreference);
    }

    private void SetCurrentTheme(ClientPreference themePreference)
    {
        _isDarkMode = themePreference.IsDarkMode;
        _drawerOpen = themePreference.IsDrawerOpen;
        StateHasChanged();
    }

    private async Task ToggleDarkMode()
    {
        _isDarkMode = !_isDarkMode;
        if (_themePreference != null)
        {
            _themePreference.IsDarkMode = _isDarkMode;
            await ClientPreferences.SetPreference(_themePreference);
        }
        await OnDarkModeToggle.InvokeAsync();
        StateHasChanged();
    }
}