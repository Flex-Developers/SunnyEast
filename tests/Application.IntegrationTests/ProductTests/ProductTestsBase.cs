using Domain.Entities;

namespace Application.IntegrationTests.ProductTests;

public class ProductTestsBase : BaseTest
{
    protected ProductCategory SampleLevel = null!;

    [SetUp]
    public async Task SetUp()
    {
        SampleLevel = await GetLevelAsync();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected async Task<ProductCategory> GetLevelAsync()
    {
        var level = await FirstOrDefaultAsync<ProductCategory>(s => true);
        if (level == null)
        {
            level = new ProductCategory
            {
                Name = "test",
                Slug = "test"
            };
            await AddAsync(level);
        }

        return level;
    }
}