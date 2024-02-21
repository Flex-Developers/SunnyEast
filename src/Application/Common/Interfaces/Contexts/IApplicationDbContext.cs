using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces.Contexts;

public interface IApplicationDbContext
{
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<IdentityUserClaim<Guid>> UserClaims { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<ShopOrder> ShopsOrders { get; set; }
    public DbSet<Order> Orders { get; set; }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    public DbSet<ApplicationUser> Users { get; set; }
}