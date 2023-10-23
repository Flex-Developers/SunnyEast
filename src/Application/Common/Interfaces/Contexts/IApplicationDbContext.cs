using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces.Contexts;

public interface IApplicationDbContext
{
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}