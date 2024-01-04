using System.Net;
using System.Net.Http.Json;
using Application.Contract.ProductCategory.Commands;
using Domain.Entities;

namespace Application.IntegrationTests.ProductCategoryTests.Commands;

public class CreateProductCategoryCommandTests : BaseTest
{
    [Test]
    public async Task CreateProductCategory_ValidRequest_ReturnsOk()
    {
        var createCommand = new CreateProductCategoryCommand
        {
            Name = "TestCategory"
        };

        var createResponse = await HttpClient.PostAsJsonAsync("/api/ProductCategory", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var category =
            await FirstOrDefaultAsync<ProductCategory>(s => s.Name == createCommand.Name);

        Assert.That(category, Is.Not.Null);
    }

    [Test]
    public async Task CreateProductCategory_Exist_ReturnsConflict()
    {
        var createCommand = new CreateProductCategoryCommand
        {
            Name = "TestCategoryConflict"
        };
        await AddAsync(new ProductCategory { Name = createCommand.Name });

        var createResponse = await HttpClient.PostAsJsonAsync("/api/ProductCategory", createCommand);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }
}