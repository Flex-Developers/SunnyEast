using Client.Infrastructure.Services.Auth;
using Client.Infrastructure.Services.Category;
using Client.Infrastructure.Services.HttpClient;
using Client.Infrastructure.Services.Product;
using Client.Infrastructure.Services.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Infrastructure.Services;

public static class DependencyInitializer
{
    public static IServiceCollection AddSunnyEastApiServices(this IServiceCollection services)
    {
        return services
            .AddScoped<ICategoryService, CategoryService>()
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IProductService, ProductService>()
            .AddScoped<IHttpClientService, HttpClientService>()
            .AddScoped<IValidationService, ValidationService>();

    }
}