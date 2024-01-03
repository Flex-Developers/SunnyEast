using System.Net;
using Domain.Entities;

namespace Application.IntegrationTests.CustomerTests.Commands;

public class DeleteCustomerCommandTests : CustomerTestsBase
{
    [Test]
    public async Task DeleteCustomer_ValidRequest_ReturnsOk()
    {
        var response = await HttpClient.DeleteAsync($"api/Customer/{await GetCustomerAsync()}");
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task DeleteCustomer_ValidRequest_ShouldRemoveFromDb()
    {
        var slug = await GetCustomerAsync();
        await HttpClient.DeleteAsync($"api/Customer/{slug}");
        var deletedCustomer = await FirstOrDefaultAsync<Customer>(s => s.Slug == slug);
        Assert.That(deletedCustomer, Is.EqualTo(null));
    }

    [Test]
    public async Task DeleteCustomer_InvalidRequest_ReturnsNotFound()
    {
        var response = await HttpClient.DeleteAsync($"api/Customer/{Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }


    private async Task<string> GetCustomerAsync()
    {
        var customer = new Customer
        {
            Slug = "slug" + new Random().Next(1, 100),
            Name = "name",
            LevelId = SampleLevel.Id,
            LevelSlug = SampleLevel.Slug
        };
        await AddAsync(customer);
        return customer.Slug;
    }
}