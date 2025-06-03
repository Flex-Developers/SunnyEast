using System.Net;
using System.Net.Http.Json;
using Application.Contract.Product.Commands;
using Domain.Entities;

namespace Application.IntegrationTests.ProductTests.Commands;

public class CreateProductTests : ProductTestsBase
{
    [Test]
    public async Task CreateProduct_ValidRequest_ReturnsOk()
    {
        var command = new CreateProductCommand
        {
            Name = "this is a name",
            ProductCategorySlug = SampleLevel.Slug,
        };
        var response = await HttpClient.PostAsJsonAsync("/api/Product", command);
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task CreateProduct_ValidRequest_ShouldSaveInDb()
    {
        var command = new CreateProductCommand
        {
            Name = "this is a name12",
            ProductCategorySlug = SampleLevel.Slug,
        };

        await HttpClient.PostAsJsonAsync("/api/Product", command);

        var product =
            await FirstOrDefaultAsync<Product>(s =>
                s.Name == command.Name && s.ProductCategorySlug == command.ProductCategorySlug);
        Assert.That(product, Is.Not.Null);
    }

    [Test]
    public async Task CreateProduct_DuplicateName_ReturnsConflictResult()
    {
        var product = new Product
        {
            Name = "name",
            ProductCategoryId = SampleLevel.Id,
            ProductCategorySlug = SampleLevel.Slug,
            Slug = "name",
        };
        await AddAsync(product);

        var command = new CreateProductCommand
        {
            Name = product.Name,
            ProductCategorySlug = SampleLevel.Slug,
        };
        var response = await HttpClient.PostAsJsonAsync("/api/Product", command);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }
}