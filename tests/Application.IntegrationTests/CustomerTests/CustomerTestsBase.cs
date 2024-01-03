using Domain.Entities;

namespace Application.IntegrationTests.CustomerTests;

public class CustomerTestsBase : BaseTest
{
    protected Level SampleLevel = null!;

    [SetUp]
    public async Task SetUp()
    {
        SampleLevel = await GetLevelAsync();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected async Task<Level> GetLevelAsync()
    {
        var level = await FirstOrDefaultAsync<Level>(s => true);
        if (level == null)
        {
            level = new Level
            {
                Name = "testLevel",
                Slug = "slug"
            };
            await AddAsync(level);
        }

        return level;
    }
}