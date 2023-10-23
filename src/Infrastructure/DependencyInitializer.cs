using Application.Common.Interfaces.Contexts;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInitializer
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configurations)
    {
        var mySqlConnectionString = configurations.GetConnectionString("mySql");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (configurations["UseInMemoryDatabase"] == "True")
                options.UseInMemoryDatabase("testDb");
            else
                options.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString));
        });

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
    }
}