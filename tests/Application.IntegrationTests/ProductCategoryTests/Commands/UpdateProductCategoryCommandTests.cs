using System.Net;
using System.Net.Http.Json;
using Application.Contract.ProductCategory.Commands;
using Domain.Entities;

namespace Application.IntegrationTests.ProductCategoryTests.Commands;

public class UpdateProductCategoryCommandTests : BaseTest
{
    [Test]
    public async Task UpdateProductCategory_ValidRequest_ReturnsOk()
    {
        var slug = await GetCategoryIdAsync("validCategory");
        var updateCommand = new UpdateProductCategoryCommand
        {
            Slug = slug,
            Name = "lsf jlkjalsk jsja;kj"
        };

        var updateResponse = await HttpClient.PutAsJsonAsync("/api/ProductCategory", updateCommand);
        updateResponse.EnsureSuccessStatusCode();
        var category =
            await FirstOrDefaultAsync<ProductCategory>(s => s.Name == updateCommand.Name);

        Assert.That(category, Is.Not.Null);
    }

    [Test]
    public async Task UpdateProductCategory_DoesntExistId_ReturnsNotFound()
    {
        var slug = await GetCategoryIdAsync("validCategory");
        var updateCommand = new UpdateProductCategoryCommand
        {
            Slug = slug,
            Name = "lsf jlkjalsk jsja;kj"
        };

        var updateResponse = await HttpClient.PutAsJsonAsync("/api/ProductCategory", updateCommand);
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdateProductCategory_Exist_ReturnsConflict()
    {
        var slug = await GetCategoryIdAsync("duplicate");
        var updateCommand = new UpdateProductCategoryCommand
        {
            Slug = slug,
            Name = "duplicate"
        };
        await AddAsync(new ProductCategory
        {
            Name = updateCommand.Name,
            Slug = "slugss"
        });

        var updateResponse = await HttpClient.PutAsJsonAsync("/api/ProductCategory", updateCommand);
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    private async Task<string> GetCategoryIdAsync(string name)
    {
        var category = await FirstOrDefaultAsync<ProductCategory>(s => s.Name == name);
        if (category != null) return category.Slug;

        category = new ProductCategory
        {
            Name = name,
            Slug = "slugss"
        };
        await AddAsync(category);

        return category.Slug;
    }
}