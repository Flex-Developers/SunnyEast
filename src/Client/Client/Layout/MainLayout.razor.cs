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
    
    private DrawerVariant _drawerVariant = DrawerVariant.Persistent;

    private void OnBpChanged(Breakpoint bp)
    {
        // всё, что уже Md (960px) и уже меньше — мобильный режим
        _drawerVariant = bp < Breakpoint.Md ? DrawerVariant.Responsive : DrawerVariant.Persistent;   
        StateHasChanged();
    }

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