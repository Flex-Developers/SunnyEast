using System.Globalization;
using Blazored.LocalStorage;
using Client.Infrastructure.Auth;
using Client.Infrastructure.Common;
using Client.Infrastructure.Consts;
using Client.Infrastructure.Preferences;
using Client.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Blazored.SessionStorage;

namespace Client.Infrastructure;

public static class Startup
{
    public const string SunnyEastClientName = "SunnyEast";

    public static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration config) =>
        services
            .AddLocalization(options => options.ResourcesPath = "Resources")
            .AddBlazoredLocalStorage()
            .AddBlazoredSessionStorage()
            .AddMudServices(configuration =>
            {
                configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
                configuration.SnackbarConfiguration.HideTransitionDuration = 100;
                configuration.SnackbarConfiguration.ShowTransitionDuration = 100;
                configuration.SnackbarConfiguration.VisibleStateDuration = 3000;
                configuration.SnackbarConfiguration.ShowCloseIcon = true;
            })
            .AddScoped<IClientPreferenceManager, ClientPreferenceManager>()
            .AutoRegisterInterfaces<IAppService>()
            .AddScoped<CustomAuthStateProvider>()
            .AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>())
            .AddAuthorizationCore(RegisterPermissionClaims)
            // Add Api Http Client.
            .AddHttpClient(SunnyEastClientName, client =>
            {
                client.DefaultRequestHeaders.AcceptLanguage.Clear();
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(CultureInfo.DefaultThreadCurrentCulture
                    ?.TwoLetterISOLanguageName);
                client.BaseAddress = new Uri(config[Config.ApiBaseUrl]!);
            })
            .Services
            .AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(SunnyEastClientName))
            .AddSunnyEastApiServices();

    private static void RegisterPermissionClaims(AuthorizationOptions options)
    {
        // options.AddPolicy(, policy => policy.RequireClaim(FSHClaims.Permission, permission.Name));
    }

    private static IServiceCollection AutoRegisterInterfaces<T>(this IServiceCollection services)
    {
        var @interface = typeof(T);

        var types = @interface
            .Assembly
            .GetExportedTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => new
            {
                Service = t.GetInterface($"I{t.Name}"),
                Implementation = t
            })
            .Where(t => t.Service != null);

        foreach (var type in types)
        {
            if (@interface.IsAssignableFrom(type.Service))
            {
                services.AddTransient(type.Service, type.Implementation);
            }
        }

        return services;
    }
}