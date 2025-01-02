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

        // Создание администратора, если он не существует
        // var adminUser = await userManager.FindByNameAsync("admin");
        var adminUser = await userManager.FindByEmailAsync("avazbekolimov722@gmail.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Name = ""
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Пароль для администратора
            result.ThrowInvalidOperationIfError();

            var addClaimResult = await userManager.AddClaimAsync(adminUser,
                new Claim(ClaimTypes.NameIdentifier, adminUser.UserName));
            addClaimResult.ThrowInvalidOperationIfError();

            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);
            addToRoleResult.ThrowInvalidOperationIfError();
        }

        // Создание продавца, если он не существует
        var salesmanUser = await userManager.FindByNameAsync("salesman");
        if (salesmanUser is null)
        {
            salesmanUser = new ApplicationUser
            {
                UserName = "salesman",
                Name = ""
            };

            var result = await userManager.CreateAsync(salesmanUser, "salesMan@123"); // Пароль для продавца
            result.ThrowInvalidOperationIfError();

            var addClaimResult = await userManager.AddClaimAsync(salesmanUser,
                new Claim(ClaimTypes.NameIdentifier, salesmanUser.UserName));
            addClaimResult.ThrowInvalidOperationIfError();

            await userManager.AddToRoleAsync(salesmanUser, ApplicationRoles.Salesman);
        }
    }
}