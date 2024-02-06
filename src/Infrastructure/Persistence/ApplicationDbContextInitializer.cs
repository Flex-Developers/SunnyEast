using Application.Contract.Identity;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence;

public class ApplicationDbContextInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
   public async Task SeedAsync()
   {
       // Создание ролей, если они не существуют
       if (!await roleManager.RoleExistsAsync(ApplicationRoles.Administrator))
       {
           await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Administrator));
       }

       // Создание администратора, если он не существует
       var adminUser = await userManager.FindByEmailAsync("admin@example.com");
       if (adminUser == null)
       {
           adminUser = new ApplicationUser
           {
               UserName = "admin@example.com",
               Email = "admin@example.com",
               Name = null
           };

           var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Пароль для администратора
           
           if (result.Succeeded)
               await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);
           else
               throw new Exception($"Failed to create admin user: {result.Errors}");
       }
   }
}