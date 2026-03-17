using Microsoft.AspNetCore.Identity;
using Hospital.Core.Enums;
using Hospital.Core.Models;

namespace Hospital.API.Data
{
    public class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            var UserManager = service.GetRequiredService<UserManager<ApplicationUser>>();
            var RoleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            string[] Roles = { enRole.Admin.ToString(), enRole.Manager.ToString(), enRole.User.ToString() };
            foreach (var role in Roles)
            {
                
                if (!await RoleManager.RoleExistsAsync(role))
                {
                    await RoleManager.CreateAsync(new IdentityRole(role));
                }
                
            }
            var adminEmail = "Admin@hospital.com";
            var UserName = await UserManager.FindByEmailAsync(adminEmail);
            if (UserName == null)
            {
                var NewAdmin = new ApplicationUser
                {
                    UserName = "Admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    FullName = "System Admin"
                };
                var result = await UserManager.CreateAsync(NewAdmin, "Admin@1234");
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(NewAdmin,enRole.Admin.ToString());
                }
            }
        }
    }
}
