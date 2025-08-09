using System.Security.Claims;
using Application.Common;
using Application.Contract.Identity;
using Domain.Entities;
using Domain.Enums;
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
            phoneNumber: "+7-999-123-45-67",
            email: "superadmin@gmail.com",
            displayName: "Super Admin",
            password: "Admin@123");
        await EnsureUserInRoleAsync(superAdmin, ApplicationRoles.SuperAdmin, exclusive: true);

        // Пример: при необходимости можно добавить универсальный e-mail claim
        await EnsureEmailClaimAsync(superAdmin);
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
    private async Task<ApplicationUser> EnsureUserAsync(string email,string phoneNumber, string displayName, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null) 
            return user;

        user = new ApplicationUser
        {
            PhoneNumber = phoneNumber,
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

        if (!exclusive) 
            return;

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
    
    private async Task EnsureStaffRecordAsync(ApplicationUser user)
    {
        // Определяем staff-роль из Identity-ролей
        var roles = await userManager.GetRolesAsync(user);

        StaffRole? role = null;
        if (roles.Contains(ApplicationRoles.Administrator, StringComparer.OrdinalIgnoreCase))
            role = StaffRole.Administrator;
        else if (roles.Contains(ApplicationRoles.Salesman, StringComparer.OrdinalIgnoreCase))
            role = StaffRole.Salesman;
        // SuperAdmin сознательно НЕ добавляем в Staff. Если нужно — раскомментируй:
        // else if (roles.Contains(ApplicationRoles.SuperAdmin, StringComparer.OrdinalIgnoreCase))
        //     role = StaffRole.Administrator;

        if (role is null)
            return; // у пользователя нет staff-роли — ничего не делаем

        var staff = await db.Staff.FirstOrDefaultAsync(s => s.UserId == user.Id);
        if (staff is null)
        {
            await db.Staff.AddAsync(new Staff
            {
                UserId    = user.Id,
                StaffRole = role.Value, // доменный enum
                IsActive  = true,
                ShopId    = null
            });
        }
        else
        {
            staff.StaffRole = role.Value;
            staff.IsActive  = true;
            // ShopId не трогаем
        }

        await db.SaveChangesAsync();
    }
}
