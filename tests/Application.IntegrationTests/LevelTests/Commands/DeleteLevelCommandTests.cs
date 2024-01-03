using System.Net;
using Domain.Entities;

namespace Application.IntegrationTests.LevelTests.Commands;

public class DeleteLevelCommandTests : LevelTestsBase
{
    [Test]
    public async Task DeleteLevel_ValidRequest_ReturnsOk()
    {
        var response = await HttpClient.DeleteAsync($"api/Level/{await GetLevelAsync()}");
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task DeleteLevel_ValidRequest_ShouldRemoveFromDb()
    {
        var slug = await GetLevelAsync();
        await HttpClient.DeleteAsync($"api/Level/{slug}");
        var deletedLevel = await FirstOrDefaultAsync<Level>(s => s.Slug == slug);
        Assert.That(deletedLevel, Is.EqualTo(null));
    }

    [Test]
    public async Task DeleteLevel_InvalidRequest_ReturnsNotFound()
    {
        var response = await HttpClient.DeleteAsync($"api/Level/{Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }


    private async Task<string> GetLevelAsync()
    {
        var level = new Level
        {
            Slug = "slug" + new Random().Next(1, 100),
            Name = "name"
        };
        await AddAsync(level);
        return level.Slug;
    }
}