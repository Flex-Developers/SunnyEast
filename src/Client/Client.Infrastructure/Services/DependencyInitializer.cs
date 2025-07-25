﻿using Client.Infrastructure.Services.Auth;
using Client.Infrastructure.Services.Category;
using Client.Infrastructure.Services.HttpClient;
using Client.Infrastructure.Services.Cart;
using Client.Infrastructure.Services.Order;
using Client.Infrastructure.Services.Price;
using Client.Infrastructure.Services.Product;
using Client.Infrastructure.Services.Shop;
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
            .AddScoped<ICartService, CartService>()
            .AddScoped<IPriceCalculatorService, PriceCalculatorService>()
            .AddScoped<IHttpClientService, HttpClientService>()
            .AddScoped<IValidationService, ValidationService>()
            .AddScoped<IShopService, ShopService>()
            .AddScoped<IOrderService, OrderService>()
            .AddScoped<ICategoryVolumesValidationService, CategoryVolumesValidationService>();

    }
}