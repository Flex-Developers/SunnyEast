using Client.Infrastructure.Preferences;
using Microsoft.AspNetCore.Components;

namespace Client.Layout;

public partial class AdminLayout
{
    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    [Parameter]
    public EventCallback OnDarkModeToggle { get; set; }

    [Parameter]
    public EventCallback<bool> OnRightToLeftToggle { get; set; }

    [EditorRequired]
    [Parameter]
    public ClientPreference ThemePreference { get; set; } = null!;

    [EditorRequired]
    [Parameter]
    public EventCallback<ClientPreference> ThemePreferenceChanged { get; set; }

    private bool _drawerOpen;

    protected override async Task OnInitializedAsync()
    {
        if (await ClientPreferences.GetPreference() is ClientPreference preference)
        {
            _drawerOpen = preference.IsDrawerOpen;
        }
    }
}