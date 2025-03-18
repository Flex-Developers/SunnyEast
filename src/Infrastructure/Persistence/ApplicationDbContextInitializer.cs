using System.Security.Claims;
using Application.Common;
using Application.Contract.Identity;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence;

public class ApplicationDbContextInitializer(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager)
{
    public async Task SeedAsync()
    {
        // Создание роли администратора, если не существует
        if (!await roleManager.RoleExistsAsync(ApplicationRoles.Administrator))
            await roleManager.CreateAsync(new IdentityRole<Guid>(ApplicationRoles.Administrator));

        // Создание роли продавца, если не существует
        if (!await roleManager.RoleExistsAsync(ApplicationRoles.Salesman))
            await roleManager.CreateAsync(new IdentityRole<Guid>(ApplicationRoles.Salesman));

        var existingUser = await userManager.FindByEmailAsync("admin@gmail.com");
        if (existingUser != null && !await userManager.IsInRoleAsync(existingUser, ApplicationRoles.Administrator))
        {
            await userManager.AddToRoleAsync(existingUser, ApplicationRoles.Administrator);
        }

        // Создание администратора, если он не существует
        var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                Email = "admin@gmail.com",
                UserName = "admin@gmail.com", // UserName теперь можно сделать таким же, как email
                Name = "Administrator",
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Пароль для администратора
            result.ThrowInvalidOperationIfError();

            // Добавление claim для email
            var addClaimResult = await userManager.AddClaimAsync(adminUser,
                new Claim(ClaimTypes.NameIdentifier, adminUser.Name));
            addClaimResult.ThrowInvalidOperationIfError();

            // Назначение роли администратора
            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);
            addToRoleResult.ThrowInvalidOperationIfError();
        }

        // Создание продавца, если он не существует
        var salesmanUser = await userManager.FindByEmailAsync("salesman@gmail.com");
        if (salesmanUser is null)
        {
            salesmanUser = new ApplicationUser
            {
                Email = "salesman@gmail.com",
                UserName = "salesman@gmail.com", // UserName теперь можно сделать таким же, как email
                Name = "Salesman"
            };

            var result = await userManager.CreateAsync(salesmanUser, "SalesMan@123"); // Пароль для продавца
            result.ThrowInvalidOperationIfError();

            // Добавление claim для email
            var addClaimResult = await userManager.AddClaimAsync(salesmanUser,
                new Claim(ClaimTypes.Email, salesmanUser.Email));
            addClaimResult.ThrowInvalidOperationIfError();

            // Назначение роли продавца
            await userManager.AddToRoleAsync(salesmanUser, ApplicationRoles.Salesman);
        }
    }
}