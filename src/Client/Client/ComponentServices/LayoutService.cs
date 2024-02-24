using Client.Enums;
using MudBlazor;

namespace Client.ComponentServices;

public class LayoutService
{
    public AppThemes CurrentThemeToggle { get; private set; } = AppThemes.System;
    public MudTheme CurrentTheme { get; } = new();
    public bool IsDarkMode;
    public bool IsSystemDark;

    public event EventHandler MajorUpdateOccured;
    private void OnMajorUpdateOccured() => MajorUpdateOccured(this, EventArgs.Empty);

    public void SetDarkMode(bool value)
    {
        CurrentThemeToggle = AppThemes.Dark;
    }

    public void ToggleDarkMode()
    {
        switch (CurrentThemeToggle)
        {
            case AppThemes.System:
                CurrentTheme.Palette = IsSystemDark ? DarkPalette : Palette;

                CurrentThemeToggle = AppThemes.Light;
                break;
            case AppThemes.Light:
                CurrentThemeToggle = AppThemes.Dark;
                CurrentTheme.Palette = DarkPalette;
                break;
            case AppThemes.Dark:
                CurrentThemeToggle = AppThemes.System;
                CurrentTheme.Palette = Palette;
                break;
        }

        Console.WriteLine(CurrentThemeToggle);
        OnMajorUpdateOccured();
    }
    
    private Palette Palette => new ()
    {
        Primary = "#FFA500", // Оранжевый
        Secondary = "#008000", // Зеленый
        Background = "#FFFFFF", // Белый для светлой темы
        Surface = "#F0F0F0", // Светло-серый для светлой темы
        DrawerBackground = "#FFFFFF", // Белый для светлой темы
        DrawerText = "rgba(0, 0, 0, 0.7)", // Темный текст для светлой темы
        AppbarBackground = "#FFA500", // Оранжевый для светлой темы
        AppbarText = "#FFFFFF", // Белый текст для светлой темы
        TextPrimary = "rgba(0, 0, 0, 0.87)", // Темный текст для светлой темы
        TextSecondary = "rgba(0, 0, 0, 0.6)", // Темно-серый текст для светлой темы
        ActionDefault = "rgba(0, 0, 0, 0.6)", // Темно-серый для светлой темы
        ActionDisabled = "rgba(0, 0, 0, 0.3)", // Серый для светлой темы
        Divider = "rgba(0, 0, 0, 0.12)" // Светло-серый для светлой темы
    };

    private Palette DarkPalette => new Palette
    {
        Primary = "#FFA500", // Оранжевый
        Secondary = "#008000", // Зеленый
        Background = "#121212", // Темный фон для темной темы
        Surface = "#333333", // Темно-серый для темной темы
        DrawerBackground = "#333333", // Темно-серый для темной темы
        DrawerText = "rgba(255, 255, 255, 0.7)", // Светлый текст для темной темы
        AppbarBackground = "#FFA500", // Оранжевый для темной темы
        AppbarText = "#FFFFFF", // Белый текст для темной темы
        TextPrimary = "rgba(255, 255, 255, 0.87)", // Светлый текст для темной темы
        TextSecondary = "rgba(255, 255, 255, 0.6)", // Светло-серый текст для темной темы
        ActionDefault = "rgba(255, 255, 255, 0.6)", // Светло-серый для темной темы
        ActionDisabled = "rgba(255, 255, 255, 0.3)", // Серый для темной темы
        Divider = "rgba(255, 255, 255, 0.12)" // Светло-серый для темной темы
    };
}