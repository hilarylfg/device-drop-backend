using device_drop_backend.Data;
using device_drop_backend.Models;

namespace device_drop_backend.Seed;

public static class SeedUsers
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var users = new List<User>
        {
            new User
            {
                FirstName = "User",
                Email = "user@test.ru",
                Password = BCrypt.Net.BCrypt.HashPassword("111111"),
                Verified = DateTime.UtcNow,
                Role = UserRole.USER,
            },
            new User
            {
                FirstName = "Admin",
                Email = "admin@test.ru",
                Password = BCrypt.Net.BCrypt.HashPassword("111111"),
                Verified = DateTime.UtcNow,
                Role = UserRole.ADMIN
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}