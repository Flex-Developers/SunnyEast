using Application.Common.Interfaces.Contexts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Contexts;
#nullable disable
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Level> Levels { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<ShopOrder> ShopOrders { get; set; }
}