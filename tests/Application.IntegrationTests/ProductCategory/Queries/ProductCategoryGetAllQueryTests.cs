using System.Net.Http.Json;
using Application.Contract.ProductCategory.Responses;

namespace Application.IntegrationTests.ProductCategory.Queries;

public class ProductCategoryGetAllQueryTests : BaseTest
{
    [Test]
    public async Task GetAllReturnsAll()
    {
        var category1 = new Domain.Entities.ProductCategory { Name = "category1" };
        var category2 = new Domain.Entities.ProductCategory { Name = "category2" };
        var category3 = new Domain.Entities.ProductCategory { Name = "category3" };
        var category4 = new Domain.Entities.ProductCategory { Name = "category4" };
        await AddAsync(category1);
        await AddAsync(category2);
        await AddAsync(category3);
        await AddAsync(category4);

        var productCategories =
            await HttpClient.GetFromJsonAsync<List<ProductCategoryResponse>>("/api/ProductCategory");
        Assert.Multiple(() =>
        {
            Assert.That(productCategories, Is.Not.Null);
            Assert.That(productCategories.Any(c => c.Name == category1.Name), Is.True);
            Assert.That(productCategories.Any(c => c.Name == category2.Name), Is.True);
            Assert.That(productCategories.Any(c => c.Name == category3.Name), Is.True);
            Assert.That(productCategories.Any(c => c.Name == category4.Name), Is.True);
        });
    }
}