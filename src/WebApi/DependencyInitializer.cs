using System.Text.Json.Serialization;
using WebApi.Filters;

namespace WebApi;

public static class DependencyInitializer
{
    public static void AddWebApi(this IServiceCollection services)
    {
        services.AddControllers(options => { options.Filters.Add<CustomExceptionsFilterAttribute>(); })
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddHttpContextAccessor();
    }
}