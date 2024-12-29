using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WebApi.Entities;

namespace WebApi.Data;

public class Seeder
{
    public static async Task SeedUsers(UserManager<User> userManager, RoleManager<AppRole> roleManager)
    {
        if (await userManager.Users.AnyAsync())
        {
            return;
        }

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var users = JsonSerializer.Deserialize<List<User>>(userData, options);

        if (users == null)
        {
            return;
        }

        var roles = new List<AppRole>
        {
            new() { Name="Member" },
            new() { Name="Admin" },
            new() { Name="Moderator" }
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        foreach (var user in users)
        {
            await userManager.CreateAsync(user, "Pa$$w0rd");
            await userManager.AddToRoleAsync(user, "Member");
        }

        var admin = new User
        {
            UserName = "admin",
            KnownAs = "Admin",
            Gender = "",
            City = "",
            Country = ""
        };

        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
    }
}
