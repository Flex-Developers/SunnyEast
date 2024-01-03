using System.Net.Http.Json;
using Application.Contract.Level.Responses;
using Domain.Entities;

namespace Application.IntegrationTests.LevelTests.Queries;

public class GetLevelTests : LevelTestsBase
{
    [Test]
    public async Task GetLevels_NoLevels_ReturnsEmptyList()
    {
        await ClearEntityAsync<Level>();
        var response = await HttpClient.GetFromJsonAsync<List<LevelResponse>>("/api/level");
        Assert.That(response?.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetLevels_EmptyRequest_ReturnsAllLevels()
    {
        await ClearEntityAsync<Level>();
        for (var i = 0; i < 100; i++) await AddLevel("slug" + i, "name" + i);

        var response = await HttpClient.GetFromJsonAsync<List<LevelResponse>>("/api/level");
        Assert.That(response?.Count, Is.EqualTo(100));
    }

    [Test]
    public async Task GetLevels_SlugRequest_ReturnsLevelWithSuggestsSlug()
    {
        await ClearEntityAsync<Level>();
        for (var i = 0; i < 100; i++) await AddLevel("slug" + i, "name" + i);

        var randomEntity = (await GetAllAsync<Level>())[new Random().Next(0, 100)];
        var response =
            await HttpClient.GetFromJsonAsync<List<LevelResponse>>($"/api/level?slug={randomEntity.Slug}");
        Assert.That(response?.FirstOrDefault()?.Slug, Is.EqualTo(randomEntity.Slug));
    }


    [Test]
    public async Task GetLevels_MixRequest_ReturnsLevelWithSuggestsSlug()
    {
        await ClearEntityAsync<Level>();
        for (var i = 0; i < 100; i++) await AddLevel("slug" + i, "name" + i);

        var randomEntity = (await GetAllAsync<Level>())[new Random().Next(0, 100)];
        var response = await HttpClient.GetFromJsonAsync<List<LevelResponse>>(
            $"/api/level?slug={randomEntity.Slug}&name={randomEntity.Name}");
        Assert.That(response?.FirstOrDefault()?.Slug, Is.EqualTo(randomEntity.Slug));
    }

    private async Task AddLevel(string slug, string name)
    {
        var level = new Level
        {
            Slug = slug,
            Name = name
        };
        await AddAsync(level);
    }
}