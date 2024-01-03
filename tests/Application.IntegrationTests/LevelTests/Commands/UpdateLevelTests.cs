using System.Net;
using System.Net.Http.Json;
using Application.Contract.Level.Commands;
using Domain.Entities;

namespace Application.IntegrationTests.LevelTests.Commands;

public class UpdateLevelTests : LevelTestsBase
{
    [Test]
    public async Task UpdateLevel_ValidRequest_ReturnsOk()
    {
        var level = await CreateOverrideSampleLevel();
        var command = new UpdateLevelCommand
        {
            Slug = level.Slug,
            Name = "newDlDs"
        };
        var response = await HttpClient.PutAsJsonAsync($"/api/level/{command.Slug}", command);
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task UpdateLevel_ValidRequest_ShouldSaveInDb()
    {
        var level = await CreateOverrideSampleLevel();
        var command = new UpdateLevelCommand
        {
            Slug = level.Slug,
            Name = "newDlDs1"
        };
        await HttpClient.PutAsJsonAsync($"/api/level/{command.Slug}", command);
        var updatedLevel = await FirstOrDefaultAsync<Level>(s => s.Name == command.Name);
        Assert.That(updatedLevel, Is.Not.Null);
    }

    [Test]
    public async Task UpdateLevel_DoesntExist_ReturnsNotFound()
    {
        var level = await CreateOverrideSampleLevel();
        var command = new UpdateLevelCommand
        {
            Slug = level.Slug + "it will not exist",
            Name = "newDlDs"
        };
        var response = await HttpClient.PutAsJsonAsync($"/api/level/{command.Slug}", command);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }


    private async Task<Level> CreateOverrideSampleLevel()
    {
        var level = new Level
        {
            IsDeleted = false,
            Name = "this is a sample name",
            Slug = "this_is_a_slug"
        };
        var old = await FirstOrDefaultAsync<Level>(s => s.Name == level.Name);
        if (old != null) await RemoveAsync(old);

        await AddAsync(level);

        return level;
    }
}