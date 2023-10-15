using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Application.IntegrationTests;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var inMemoryConfiguration = GetInMemoryConfiguration();
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var integrationConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemoryConfiguration!)
                .AddEnvironmentVariables()
                .Build();
            configurationBuilder.AddConfiguration(integrationConfig);
        });
    }

    private List<KeyValuePair<string, string>> GetInMemoryConfiguration()
    {
        List<KeyValuePair<string, string>> inMemoryConfiguration = new()
        {
            new KeyValuePair<string, string>("UseInMemoryDatabase", true.ToString())
        };
        return inMemoryConfiguration;
    }
}