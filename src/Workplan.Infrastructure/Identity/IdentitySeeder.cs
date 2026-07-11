using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workplan.Domain.Common;
using Workplan.SharedKernel.Auth;

namespace Workplan.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role) { Id = EntityId.New() });
        }

        var adminSection = configuration.GetSection("InitialAdmin");
        var email = adminSection["Email"];
        var password = adminSection["Password"];
        var fullName = adminSection["FullName"] ?? "System Admin";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new ApplicationUser { Id = EntityId.New(), UserName = email, Email = email, FullName = fullName };
        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, Roles.SystemAdmin);
    }
}
