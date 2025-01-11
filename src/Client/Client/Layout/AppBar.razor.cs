using Client.Infrastructure.Preferences;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;

namespace Client.Layout;

public partial class AppBar
{
    [Parameter] public RenderFragment ChildContent { get; set; } = default!;
    [Parameter] public EventCallback OnDarkModeToggle { get; set; }
    [Parameter] public EventCallback<bool> OnRightToLeftToggle { get; set; }
    [EditorRequired] [Parameter] public ClientPreference ThemePreference { get; set; } = default!;
    [EditorRequired] [Parameter] public EventCallback<ClientPreference> ThemePreferenceChanged { get; set; }

    private bool _drawerOpen;
    private bool _rightToLeft;

    protected override async Task OnInitializedAsync()
    {
        if (await ClientPreferences.GetPreference() is ClientPreference preference)
        {
            _rightToLeft = preference.IsRTL;
            _drawerOpen = preference.IsDrawerOpen;
        }
    }

    private async Task RightToLeftToggle()
    {
        bool isRtl = await ClientPreferences.ToggleLayoutDirectionAsync();
        _rightToLeft = isRtl;

        await OnRightToLeftToggle.InvokeAsync(isRtl);
    }

    public async Task ToggleDarkMode()
    {
        await OnDarkModeToggle.InvokeAsync();
    }

    private async Task DrawerToggle()
    {
        _drawerOpen = await ClientPreferences.ToggleDrawerAsync();
    }

    private void Logout()
    {
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        DialogService.Show<Components.Dialogs.Logout>("Logout", options);
    }

    private void Profile()
    {
        Navigation.NavigateTo("/account");
    }

    private async Task ToggleDarkLightMode(bool isDarkMode)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.IsDarkMode = isDarkMode;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private void SignUp()
    {
        Navigation.NavigateToLogin("/register");
    }

    private void SignIn()
    {
        Navigation.NavigateToLogin("/login");
    }
}