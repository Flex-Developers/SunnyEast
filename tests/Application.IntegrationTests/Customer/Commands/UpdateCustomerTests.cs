using System.Net;
using System.Net.Http.Json;
using Application.Contract.Customer.Commands;

namespace Application.IntegrationTests.Customer.Commands;

public class UpdateCustomerTests : BaseTest
{
    [Test]
    public async Task UpdateCustomer_ValidRequest_ReturnsOk()
    {
        var customer = await CreateOverrideSampleCustomer();
        var command = new UpdateCustomerCommand
        {
            Slug = customer.Slug,
            Phone = "newPhone",
            Name = "newDlDs"
        };
        var response = await HttpClient.PutAsJsonAsync($"/api/customers/{command.Slug}", command);
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task UpdateCustomer_ValidRequest_ShouldSaveInDb()
    {
        var customer = await CreateOverrideSampleCustomer();
        var command = new UpdateCustomerCommand
        {
            Slug = customer.Slug,
            Phone = "newPhone1",
            Name = "newDlDs1"
        };
        await HttpClient.PutAsJsonAsync($"/api/customers/{command.Slug}", command);
        var updatedCustomer = await FirstOrDefaultAsync<Domain.Entities.Customer>(s => s.Name == command.Name);
        Assert.That(updatedCustomer, Is.Not.Null);
    }

    [Test]
    public async Task UpdateCustomer_DoesntExist_ReturnsNotFound()
    {
        var customer = await CreateOverrideSampleCustomer();
        var command = new UpdateCustomerCommand
        {
            Slug = customer.Slug + "it will not exist",
            Phone = "newPhone",
            Name = "newDlDs"
        };
        var response = await HttpClient.PutAsJsonAsync($"/api/customers/{command.Slug}", command);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }


    private async Task<Domain.Entities.Customer> CreateOverrideSampleCustomer()
    {
        var customer = new Domain.Entities.Customer
        {
            IsDeleted = false,
            Name = "this is a sample name",
            Phone = "this is a phone",
            Slug = "this_is_a_slug"
        };
        var old = await FirstOrDefaultAsync<Domain.Entities.Customer>(s => s.Name == customer.Name);
        if (old != null) await RemoveAsync(old);

        await AddAsync(customer);

        return customer;
    }
}