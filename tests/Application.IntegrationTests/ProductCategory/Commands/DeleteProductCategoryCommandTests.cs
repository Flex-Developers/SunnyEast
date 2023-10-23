using System.Net;

namespace Application.IntegrationTests.ProductCategory.Commands;

public class DeleteProductCategoryCommandTests : BaseTest
{
    [Test]
    public async Task DeleteProductCategory_ValidRequest_ReturnsOk()
    {
        var categoryId = await GetCategoryIdAsync();

        var deleteResponse =
            await HttpClient.DeleteAsync($"/api/ProductCategory?id={categoryId}");
        deleteResponse.EnsureSuccessStatusCode();
        var category =
            await FindFirstOrDefaultAsync<Domain.Entities.ProductCategory>(s => s.Id == categoryId);

        Assert.That(category, Is.Null);
    }

    [Test]
    public async Task DeleteProductCategory_DoesntExist_ReturnsNotFound()
    {
        var deleteResponse =
            await HttpClient.DeleteAsync($"/api/ProductCategory?id={Guid.NewGuid()}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private static async Task<Guid> GetCategoryIdAsync()
    {
        var category = await FindFirstOrDefaultAsync<Domain.Entities.ProductCategory>(s => s.Name == " FasdfdsfasdF ");
        if (category != null) return category.Id;

        category = new Domain.Entities.ProductCategory
        {
            Name = " FasdfdsfasdF "
        };
        await AddAsync(category);

        return category.Id;
    }
}