﻿using System.Text.RegularExpressions;
using Blazored.LocalStorage;
using Client.Infrastructure.Theme;

namespace Client.Infrastructure.Preferences;

public class ClientPreferenceManager : IClientPreferenceManager
{
    private readonly ILocalStorageService _localStorageService;

    public ClientPreferenceManager(
        ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public async Task<bool> ToggleDarkModeAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            preference.IsDarkMode = !preference.IsDarkMode;
            await SetPreference(preference);
            return !preference.IsDarkMode;
        }

        return false;
    }

    public async Task<bool> ToggleDrawerAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            preference.IsDrawerOpen = !preference.IsDrawerOpen;
            await SetPreference(preference);
            return preference.IsDrawerOpen;
        }

        return false;
    }

    public async Task<string> GetPrimaryColorAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            string colorCode = preference.PrimaryColor;

            if (Regex.Match(colorCode, "^#(?:[0-9a-fA-F]{3,4}){1,2}$").Success)
            {
                return colorCode;
            }
            else
            {
                preference.PrimaryColor = CustomColors.Light.PrimaryGreen;
                await SetPreference(preference);
                return preference.PrimaryColor;
            }
        }

        return CustomColors.Light.PrimaryGreen;
    }

    public async Task<bool> IsDrawerOpen()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            return preference.IsDrawerOpen;
        }

        return false;
    }

    public static string Preference = "clientPreference";

    public async Task<IPreference> GetPreference()
    {
        return await _localStorageService.GetItemAsync<ClientPreference>(Preference) ?? new ClientPreference();
    }

    public async Task SetPreference(IPreference preference)
    {
        await _localStorageService.SetItemAsync(Preference, preference as ClientPreference);
    }
}