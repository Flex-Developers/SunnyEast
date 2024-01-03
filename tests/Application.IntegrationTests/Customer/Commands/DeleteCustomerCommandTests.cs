using System.Net;

namespace Application.IntegrationTests.Customer.Commands;

public class DeleteCustomerCommandTests : BaseTest
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
        var deletedCustomer = await FirstOrDefaultAsync<Domain.Entities.Customer>(s => s.Slug == slug);
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
        var customer = new Domain.Entities.Customer
        {
            Slug = "slug" + new Random().Next(1, 100),
            Name = "name"
        };
        await AddAsync(customer);
        return customer.Slug;
    }
}