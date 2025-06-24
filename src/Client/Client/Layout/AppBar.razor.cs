using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;

namespace Client.Layout;

public partial class AppBar : IDisposable
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
    
    private int _cartCount;

    protected override async Task OnInitializedAsync()
    {
        _cartCount = await CartService.GetOrdersCountAsync();
        CartService.OnChange += CartChanged;
    }

    private async void CartChanged()
    {
        _cartCount = await CartService.GetOrdersCountAsync();
        StateHasChanged();
    }


    private void Logout()
    {
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        DialogService.Show<Components.Dialogs.Logout>("Выход", options);
    }

    private void Profile() => Navigation.NavigateTo("/account");
    private void SignUp() => Navigation.NavigateToLogin("/register");
    private void SignIn() => Navigation.NavigateToLogin("/login");
    
    public void Dispose()
    {
        CartService.OnChange -= CartChanged;
    }
}