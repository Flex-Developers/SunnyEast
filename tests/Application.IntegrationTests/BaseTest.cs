using System.Linq.Expressions;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests;

public class BaseTest
{
    private static WebApplicationFactory<Program> _factory = null!;
    private static IServiceScopeFactory _scopeFactory = null!;
    protected HttpClient HttpClient = null!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _factory = new CustomWebApplicationFactory();
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        _factory.Services.GetRequiredService<IConfiguration>();
    }

    [SetUp]
    public void Setup()
    {
        HttpClient = _factory.CreateClient();
    }

    protected async Task ClearEntityAsync<TEntity>() where TEntity : BaseEntity
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Set<TEntity>().RemoveRange(context.Set<TEntity>());
        await context.Set<TEntity>().ExecuteDeleteAsync();
    }

    protected async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.AddAsync(entity);

        await context.SaveChangesAsync();
    }

    protected async Task RemoveAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Remove(entity);

        await context.SaveChangesAsync();
    }

    protected async Task<int> CountAsync<TEntity>() where TEntity : BaseEntity
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    protected async Task<TEntity?> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> criteria)
        where TEntity : BaseEntity
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.Set<TEntity>().FirstOrDefaultAsync(criteria);
    }

    protected async Task<List<TEntity>> GetAllAsync<TEntity>(Expression<Func<TEntity, bool>> criteria)
        where TEntity : BaseEntity
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.Set<TEntity>().Where(criteria).ToListAsync();
    }

    protected async Task<List<TEntity>> GetAllAsync<TEntity>() where TEntity : BaseEntity
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.Set<TEntity>().ToListAsync();
    }
}