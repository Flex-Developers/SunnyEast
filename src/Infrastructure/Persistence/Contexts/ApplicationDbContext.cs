using Application.Common.Interfaces.Contexts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Contexts;
#nullable disable
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<ShopOrder> ShopsOrders { get; set; }
    public new DbSet<ApplicationUser> Users { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<ShopOrder> ShopOrders { get; set; }
}