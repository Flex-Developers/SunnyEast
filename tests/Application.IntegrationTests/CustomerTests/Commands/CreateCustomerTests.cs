using System.Net;
using System.Net.Http.Json;
using Application.Contract.Customer.Commands;
using Domain.Entities;

namespace Application.IntegrationTests.CustomerTests.Commands;

public class CreateCustomerTests : CustomerTestsBase
{
    [Test]
    public async Task CreateCustomer_ValidRequest_ReturnsOk()
    {
        var command = new CreateCustomerCommand
        {
            Name = "this is a name",
            Phone = "923331113",
            LevelSlug = SampleLevel.Slug
        };
        var response = await HttpClient.PostAsJsonAsync("/api/Customer", command);
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task CreateCustomer_ValidRequest_ShouldSaveInDb()
    {
        var command = new CreateCustomerCommand
        {
            Name = "this is a name12",
            Phone = "92333111213",
            LevelSlug = SampleLevel.Slug
        };

        await HttpClient.PostAsJsonAsync("/api/Customer", command);

        var customer =
            await FirstOrDefaultAsync<Customer>(s =>
                s.Name == command.Name && s.Phone == command.Phone);
        Assert.That(customer, Is.Not.Null);
    }

    [Test]
    public async Task CreateCustomer_DuplicateName_ReturnsConflictResult()
    {
        var customer = new Customer
        {
            Name = "name",
            Phone = "phone",
            Slug = "name",
            LevelId = SampleLevel.Id,
            LevelSlug = SampleLevel.Slug
        };
        await AddAsync(customer);

        var command = new CreateCustomerCommand
        {
            Name = customer.Name,
            Phone = customer.Phone,
            LevelSlug = SampleLevel.Slug
        };
        var response = await HttpClient.PostAsJsonAsync("/api/Customer", command);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }
}