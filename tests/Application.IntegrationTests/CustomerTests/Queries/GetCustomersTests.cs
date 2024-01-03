using System.Net.Http.Json;
using Application.Contract.Customer.Responses;
using Domain.Entities;

namespace Application.IntegrationTests.CustomerTests.Queries;

public class GetCustomersTests : CustomerTestsBase
{
    [Test]
    public async Task GetCustomers_NoCustomers_ReturnsEmptyList()
    {
        await ClearEntityAsync<Customer>();
        var response = await HttpClient.GetFromJsonAsync<List<CustomerResponse>>("/api/customer");
        Assert.That(response?.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetCustomers_EmptyRequest_ReturnsAllCustomers()
    {
        await ClearEntityAsync<Customer>();
        for (var i = 0; i < 100; i++) await AddCustomer("slug" + i, "name" + i, "phone" + i);

        var response = await HttpClient.GetFromJsonAsync<List<CustomerResponse>>("/api/customer");
        Assert.That(response?.Count, Is.EqualTo(100));
    }

    [Test]
    public async Task GetCustomers_SlugRequest_ReturnsCustomerWithSuggestsSlug()
    {
        await ClearEntityAsync<Customer>();
        for (var i = 0; i < 100; i++) await AddCustomer("slug" + i, "name" + i, "phone" + i);

        var randomEntity = (await GetAllAsync<Customer>())[new Random().Next(0, 100)];
        var response =
            await HttpClient.GetFromJsonAsync<List<CustomerResponse>>($"/api/customer?slug={randomEntity.Slug}");
        Assert.That(response?.FirstOrDefault()?.Slug, Is.EqualTo(randomEntity.Slug));
    }


    [Test]
    public async Task GetCustomers_MixRequest_ReturnsCustomerWithSuggestsSlug()
    {
        await ClearEntityAsync<Customer>();
        for (var i = 0; i < 100; i++) await AddCustomer("slug" + i, "name" + i, "phone" + i);

        var randomEntity = (await GetAllAsync<Customer>())[new Random().Next(0, 100)];
        var response = await HttpClient.GetFromJsonAsync<List<CustomerResponse>>(
            $"/api/customer?slug={randomEntity.Slug}&Phone={randomEntity.Phone}&name={randomEntity.Name}");
        Assert.That(response?.FirstOrDefault()?.Slug, Is.EqualTo(randomEntity.Slug));
    }

    private async Task AddCustomer(string slug, string name, string? phone = null)
    {
        var customer = new Customer
        {
            Slug = slug,
            Name = name,
            Phone = phone,
            LevelId = SampleLevel.Id,
            LevelSlug = SampleLevel.Slug
        };
        await AddAsync(customer);
    }
}