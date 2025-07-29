using System.Text.Json.Serialization;
using Application.Contract.Order;
using Microsoft.AspNetCore.Http.Features;
using WebApi.Services;
using Application.Contract.Order.Hub;
using Infrastructure.Services.Order;
using Microsoft.OpenApi.Models;
using WebApi.Filters;

namespace WebApi;

public static class DependencyInitializer
{
    public static void AddWebApi(this IServiceCollection services)
    {
        services.AddControllers(options => { options.Filters.Add<CustomExceptionsFilterAttribute>(); })
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(s =>
        {
            s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
            });

            s.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });
        
        services.Configure<FormOptions>(o =>
        {
            o.MultipartBodyLengthLimit = long.MaxValue;
            o.ValueLengthLimit = int.MaxValue;
            o.MultipartHeadersCountLimit = int.MaxValue;
            o.MultipartHeadersLengthLimit = int.MaxValue;
        });
        
        services.AddHttpContextAccessor();
        services.AddSignalR();
        services.AddScoped<IOrderRealtimeNotifier, OrderRealtimeNotifier>();
    }
}