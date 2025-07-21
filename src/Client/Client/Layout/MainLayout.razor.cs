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

    protected override async Task OnInitializedAsync()
    {
        _themePreference = await ClientPreferences.GetPreference() as ClientPreference ?? new ClientPreference();

        SetCurrentTheme(_themePreference);
    }

    private void SetCurrentTheme(ClientPreference themePreference)
    {
        _isDarkMode = themePreference.IsDarkMode;
        _drawerOpen = themePreference.IsDrawerOpen;
        StateHasChanged();
    }
}