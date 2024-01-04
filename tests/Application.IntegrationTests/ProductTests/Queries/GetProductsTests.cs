using System.Net.Http.Json;
using Application.Contract.Product.Responses;
using Domain.Entities;

namespace Application.IntegrationTests.ProductTests.Queries;

public class GetProductsTests : ProductTestsBase
{
    [Test]
    public async Task GetProducts_NoProducts_ReturnsEmptyList()
    {
        await ClearEntityAsync<Product>();
        var response = await HttpClient.GetFromJsonAsync<List<ProductResponse>>("/api/product");
        Assert.That(response?.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetProducts_EmptyRequest_ReturnsAllProducts()
    {
        await ClearEntityAsync<Product>();
        for (var i = 0; i < 100; i++) await AddProduct("slug" + i, "name" + i);

        var response = await HttpClient.GetFromJsonAsync<List<ProductResponse>>("/api/product");
        Assert.That(response?.Count, Is.EqualTo(100));
    }

    [Test]
    public async Task GetProducts_SlugRequest_ReturnsProductWithSuggestsSlug()
    {
        await ClearEntityAsync<Product>();
        for (var i = 0; i < 100; i++) await AddProduct("slug" + i, "name" + i);

        var randomEntity = (await GetAllAsync<Product>())[new Random().Next(0, 100)];
        var response =
            await HttpClient.GetFromJsonAsync<List<ProductResponse>>($"/api/product?slug={randomEntity.Slug}");
        Assert.That(response?.FirstOrDefault()?.Slug, Is.EqualTo(randomEntity.Slug));
    }


    [Test]
    public async Task GetProducts_MixRequest_ReturnsProductWithSuggestsSlug()
    {
        await ClearEntityAsync<Product>();
        for (var i = 0; i < 100; i++) await AddProduct("slug" + i, "name" + i);

        var randomEntity = (await GetAllAsync<Product>())[new Random().Next(0, 100)];
        var response = await HttpClient.GetFromJsonAsync<List<ProductResponse>>(
            $"/api/product?slug={randomEntity.Slug}&name={randomEntity.Name}");
        Assert.That(response?.FirstOrDefault()?.Slug, Is.EqualTo(randomEntity.Slug));
    }

    private async Task AddProduct(string slug, string name)
    {
        var product = new Product
        {
            Slug = slug,
            Name = name,
            ProductCategoryId = SampleLevel.Id,
            ProductCategorySlug = SampleLevel.Slug,
            Price = 21312
        };
        await AddAsync(product);
    }
}