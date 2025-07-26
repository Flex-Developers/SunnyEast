using System.Security.Claims;
using Application.Common;
using Application.Contract.Identity;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Первичная инициализация базы: базовые сущности, роли и тестовые пользователи.
/// </summary>
public sealed class ApplicationDbContextInitializer(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ApplicationDbContext db)
{
    private const string AllProductsSlug = "AllProducts";

    public async Task SeedAsync()
    {
        // 1) Базовые справочники
        await EnsureBaseCategoryAsync();

        // 2) Роли
        await EnsureRolesAsync();

        // 3) Пользователи и их роли (эксклюзивно — только одна целевая роль)
        var superAdmin = await EnsureUserAsync(
            email: "superadmin@gmail.com",
            displayName: "Super Admin",
            password: "Admin@123");
        await EnsureUserInRoleAsync(superAdmin, ApplicationRoles.SuperAdmin, exclusive: true);

        var admin = await EnsureUserAsync(
            email: "admin@gmail.com",
            displayName: "Admin",
            password: "Admin@123");
        await EnsureUserInRoleAsync(admin, ApplicationRoles.Administrator, exclusive: true);

        var salesman = await EnsureUserAsync(
            email: "salesman@gmail.com",
            displayName: "Salesman",
            password: "SalesMan@123");
        await EnsureUserInRoleAsync(salesman, ApplicationRoles.Salesman, exclusive: true);

        // Пример: при необходимости можно добавить универсальный e-mail claim
        await EnsureEmailClaimAsync(superAdmin);
        await EnsureEmailClaimAsync(admin);
        await EnsureEmailClaimAsync(salesman);
    }

    /// <summary>
    /// Создаёт "Все продукты" при первом запуске.
    /// </summary>
    private async Task EnsureBaseCategoryAsync()
    {
        var exists = await db.ProductCategories.AnyAsync(c => c.Slug == AllProductsSlug);
        if (exists) return;

        db.ProductCategories.Add(new ProductCategory
        {
            Id = Guid.NewGuid(),
            Name = "Все продукты",
            Slug = AllProductsSlug
        });

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Гарантирует наличие всех используемых ролей.
    /// </summary>
    private async Task EnsureRolesAsync()
    {
        var roles = new[]
        {
            ApplicationRoles.SuperAdmin,
            ApplicationRoles.Administrator,
            ApplicationRoles.Salesman
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    /// <summary>
    /// Возвращает существующего пользователя или создаёт нового.
    /// </summary>
    private async Task<ApplicationUser> EnsureUserAsync(string email, string displayName, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null) return user;

        user = new ApplicationUser
        {
            Email = email,
            UserName = email,      // держим UserName = email для простоты логина
            Name = displayName
        };

        var result = await userManager.CreateAsync(user, password);
        result.ThrowInvalidOperationIfError();

        return user;
    }

    /// <summary>
    /// Убеждаемся, что пользователь имеет указанную роль.
    /// Если exclusive = true — удаляем все остальные роли у пользователя.
    /// </summary>
    private async Task EnsureUserInRoleAsync(ApplicationUser user, string requiredRole, bool exclusive)
    {
        var currentRoles = await userManager.GetRolesAsync(user);

        if (!currentRoles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase))
            await userManager.AddToRoleAsync(user, requiredRole);

        if (!exclusive) return;

        var rolesToRemove = currentRoles
            .Where(r => !string.Equals(r, requiredRole, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (rolesToRemove.Length > 0)
            await userManager.RemoveFromRolesAsync(user, rolesToRemove);
    }

    /// <summary>
    /// Добавляет claim с e-mail, если его ещё нет.
    /// (Не обязателен для работы токенов — просто пример консистентности данных.)
    /// </summary>
    private async Task EnsureEmailClaimAsync(ApplicationUser user)
    {
        var claims = await userManager.GetClaimsAsync(user);
        var hasEmailClaim = claims.Any(c => c.Type == ClaimTypes.Email && c.Value == user.Email);

        if (!hasEmailClaim && !string.IsNullOrWhiteSpace(user.Email))
        {
            var addEmailClaim = await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, user.Email));
            addEmailClaim.ThrowInvalidOperationIfError();
        }
    }
}
