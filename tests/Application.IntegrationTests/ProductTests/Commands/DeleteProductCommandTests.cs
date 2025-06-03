using System.Net;
using Domain.Entities;

namespace Application.IntegrationTests.ProductTests.Commands;

public class DeleteProductCommandTests : ProductTestsBase
{
    [Test]
    public async Task DeleteProduct_ValidRequest_ReturnsOk()
    {
        var response = await HttpClient.DeleteAsync($"api/Product/{await GetProductAsync()}");
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task DeleteProduct_ValidRequest_ShouldRemoveFromDb()
    {
        var slug = await GetProductAsync();
        await HttpClient.DeleteAsync($"/api/Product/{slug}");
        var deletedProduct = await FirstOrDefaultAsync<Product>(s => s.Slug == slug);
        Assert.That(deletedProduct, Is.EqualTo(null));
    }

    [Test]
    public async Task DeleteProduct_InvalidRequest_ReturnsNotFound()
    {
        var response = await HttpClient.DeleteAsync($"api/Product/{Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }


    private async Task<string> GetProductAsync()
    {
        var product = new Product
        {
            Slug = $"slug{new Random().Next(1, 100)}",
            Name = "name",
            ProductCategoryId = SampleLevel.Id,
            ProductCategorySlug = SampleLevel.Slug,
        };
        await AddAsync(product);
        return product.Slug;
    }
}