namespace Client.Infrastructure.Preferences;

public interface IClientPreferenceManager : IPreferenceManager
{
    Task<bool> ToggleDarkModeAsync();

    Task<bool> ToggleDrawerAsync();

    Task<bool> ToggleLayoutDirectionAsync();
}