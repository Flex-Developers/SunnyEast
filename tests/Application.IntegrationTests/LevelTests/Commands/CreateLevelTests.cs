using System.Net;
using System.Net.Http.Json;
using Application.Contract.Level.Commands;
using Domain.Entities;

namespace Application.IntegrationTests.LevelTests.Commands;

public class CreateLevelTests : LevelTestsBase
{
    [Test]
    public async Task CreateLevel_ValidRequest_ReturnsOk()
    {
        var command = new CreateLevelCommand
        {
            Name = "this is a name"
        };
        var response = await HttpClient.PostAsJsonAsync("/api/Level", command);
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task CreateLevel_ValidRequest_ShouldSaveInDb()
    {
        var command = new CreateLevelCommand
        {
            Name = "this is a name12"
        };

        await HttpClient.PostAsJsonAsync("/api/Level", command);

        var level = await FirstOrDefaultAsync<Level>(s => s.Name == command.Name);
        Assert.That(level, Is.Not.Null);
    }

    [Test]
    public async Task CreateLevel_DuplicateName_ReturnsConflictResult()
    {
        var level = new Level
        {
            Name = "name",
            Slug = "name"
        };
        await AddAsync(level);

        var command = new CreateLevelCommand
        {
            Name = level.Name
        };
        var response = await HttpClient.PostAsJsonAsync("/api/Level", command);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }
}