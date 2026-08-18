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
        public static async Task SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedRoles(roleManager);
        }

        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            var roles = new string[3] { ApplicationRoles.Admin, ApplicationRoles.Tester, ApplicationRoles.User };
            foreach (string role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    IdentityRole new_role = new IdentityRole() { Name = role, NormalizedName = role };
                    IdentityResult roleResult = await roleManager.CreateAsync(new_role);
                }
            }
        }
    }
}
