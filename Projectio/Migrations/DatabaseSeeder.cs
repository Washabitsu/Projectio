using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Projectio.Core.Models;
using Projectio.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Xml;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projectio.Core.Enums;

namespace Projectio.Migrations
{
    public static class AppDbContextSeed
    {
        public static void SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager);
        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            var roles = new string[3] { ApplicationRoles.Admin, ApplicationRoles.Tester, ApplicationRoles.User };
            foreach (string role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    IdentityRole new_role = new IdentityRole() { Name = role, NormalizedName = role };
                    IdentityResult roleResult = roleManager.CreateAsync(new_role).Result;
                }
            }
        }

        public static void SeedUsers(UserManager<ApplicationUser> userManager)
        {
            if (userManager.FindByNameAsync("admin").Result == null)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    UserName = "admin",
                    Email = "dimitris.argyropoulos@outlook.com",
                    NormalizedUserName = "ADMIN",
                    EmailConfirmed = true
                };

                IdentityResult result = userManager.CreateAsync
                (user, "DefaultPass").Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, ApplicationRoles.Admin).Wait();
                }
            }
        }
    }
}
