using Application.Contract.Identity;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence;

public class ApplicationDbContextInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
   public async Task SeedAsync()
   {
       // Создание роли администратора, если не существует
       if (!await roleManager.RoleExistsAsync(ApplicationRoles.Administrator))
           await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Administrator));
       
       // Создание роли продавца, если не существует
       if(!await roleManager.RoleExistsAsync(ApplicationRoles.Salesman))
           await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Salesman));

       // Создание администратора, если он не существует
       var adminUser = await userManager.FindByNameAsync("admin");
       if (adminUser == null)
       {
           adminUser = new ApplicationUser
           {
               UserName = "admin",
               Name = null
           };

           var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Пароль для администратора
           
           if (result.Succeeded)
               await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);
           else
               throw new InvalidOperationException($"Failed to create admin user: {result.Errors}");
       }
       
       // Создание продавца, если он не существует
       var salesmanUser = await userManager.FindByNameAsync("salesman");
       if (salesmanUser is null)
       {
           salesmanUser = new ApplicationUser
           {
               UserName = "salesman",
               Name = null
           };

           var result = await userManager.CreateAsync(salesmanUser, "salesman@123"); // Пароль для продавца

           if (result.Succeeded)
               await userManager.AddToRoleAsync(salesmanUser, ApplicationRoles.Salesman);
           else
               throw new InvalidOperationException("Failed to create salesman user");
       }
   }
}