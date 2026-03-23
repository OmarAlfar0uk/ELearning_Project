using Auth.Models;
using ELearningProject.Models;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Seeding
{
    public static class SuperAdminSeeder
    {
        public static async Task SeedSuperAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            const string roleName = "SuperAdmin";
            const string email = "suuperadmin@university.com";
            const string password = "SuperAdmin@123";

         

            // 1️⃣ Ensure role exists
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    Description = "System Owner"
                });
            }

            // 2️⃣ Check if user exists
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return;
            
            // 3️⃣ Create SuperAdmin
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = "Omar",
                LastName = "Alfarouk",
                Gender = Gender.Male,
                IsActivated = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Failed to create SuperAdmin: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }

            // 4️⃣ Assign role
            await userManager.AddToRoleAsync(user, roleName);
        }
    }
}
