using Client.Infrastructure.Theme;

namespace Client.Infrastructure.Preferences;

public class ClientPreference : IPreference
{
    public bool IsDarkMode { get; set; }
    public bool IsDrawerOpen { get; set; }
    public string PrimaryColor { get; set; } = CustomColors.Light.PrimaryGreen;
    public string SecondaryColor { get; set; } = CustomColors.Light.SecondaryYellow;
    public double BorderRadius { get; set; } = 5;
}