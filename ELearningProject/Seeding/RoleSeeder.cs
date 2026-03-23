using Auth.Models;
using Microsoft.AspNetCore.Identity;

namespace Auth.Data.Seeding
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            string[] roles =
            {
                "SuperAdmin",
                "Admin",
                "Student"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = role,
                        Description = $"{role} role"
                    });
                }
            }
        }
    }
}
