using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;

namespace Client.Layout;

public partial class AppBar
{
    [Parameter] public RenderFragment ChildContent { get; set; } = null!;
    [Parameter] public EventCallback<bool> IsDarkModeChanged { get; set; }
    [Parameter] public bool IsDarkMode { get; set; }

    public async Task ToggleDarkLightMode()
    {
        var newState = await ClientPreferences.ToggleDarkModeAsync();
        await IsDarkModeChanged.InvokeAsync(newState);
    }

    [Parameter] public bool DrawerOpen { get; set; }

    [Parameter] public EventCallback<bool> DrawerOpenChanged { get; set; }

    private async Task DrawerToggle()
    {
        var newState = await ClientPreferences.ToggleDrawerAsync();
        await DrawerOpenChanged.InvokeAsync(newState);
    }

    private void Logout()
    {
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        DialogService.Show<Components.Dialogs.Logout>("Выход", options);
    }

    private void Profile() => Navigation.NavigateTo("/account");
    private void SignUp() => Navigation.NavigateToLogin("/register");
    private void SignIn() => Navigation.NavigateToLogin("/login");
}