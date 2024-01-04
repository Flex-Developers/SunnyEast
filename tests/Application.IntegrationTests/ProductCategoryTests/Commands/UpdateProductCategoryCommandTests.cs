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
        var categoryId = await GetCategoryIdAsync("validCategory");
        var updateCommand = new UpdateProductCategoryCommand
        {
            Id = categoryId,
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
        await GetCategoryIdAsync("validCategory");
        var updateCommand = new UpdateProductCategoryCommand
        {
            Id = Guid.NewGuid(),
            Name = "lsf jlkjalsk jsja;kj"
        };

        var updateResponse = await HttpClient.PutAsJsonAsync("/api/ProductCategory", updateCommand);
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdateProductCategory_Exist_ReturnsConflict()
    {
        var categoryId = await GetCategoryIdAsync("duplicate");
        var updateCommand = new UpdateProductCategoryCommand
        {
            Id = categoryId,
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

    private async Task<Guid> GetCategoryIdAsync(string name)
    {
        var category = await FirstOrDefaultAsync<ProductCategory>(s => s.Name == name);
        if (category != null) return category.Id;

        category = new ProductCategory
        {
            Name = name,
            Slug = "slugss"
        };
        await AddAsync(category);

        return category.Id;
    }
}