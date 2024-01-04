using System.Net.Http.Json;
using Application.Contract.ProductCategory.Responses;
using Domain.Entities;

namespace Application.IntegrationTests.ProductCategoryTests.Queries;

public class ProductCategoryGetAllQueryTests : BaseTest
{
    [Test]
    public async Task GetAllReturnsAll()
    {
        var category1 = new ProductCategory
        {
            Name = "category1",
            Slug = "das"
        };
        var category2 = new ProductCategory
        {
            Name = "category2",
            Slug = "adsfa"
        };
        var category3 = new ProductCategory
        {
            Name = "category3",
            Slug = "adsfasdf"
        };
        var category4 = new ProductCategory
        {
            Name = "category4",
            Slug = "asdfsadsdssd"
        };
        await AddAsync(category1);
        await AddAsync(category2);
        await AddAsync(category3);
        await AddAsync(category4);

        var productCategories =
            await HttpClient.GetFromJsonAsync<List<ProductCategoryResponse>>("/api/ProductCategory");
        Assert.Multiple(() =>
        {
            Assert.That(productCategories, Is.Not.Null);
            Assert.That(productCategories != null && productCategories.Any(c => c.Name == category1.Name), Is.True);
            Assert.That(productCategories.Any(c => c.Name == category2.Name), Is.True);
            Assert.That(productCategories.Any(c => c.Name == category3.Name), Is.True);
            Assert.That(productCategories.Any(c => c.Name == category4.Name), Is.True);
        });
    }
}