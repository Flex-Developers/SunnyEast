using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces.Contexts;

public interface IApplicationDbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Level> Levels { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}