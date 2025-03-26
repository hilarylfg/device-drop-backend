using device_drop_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace device_drop_backend.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await DownAsync(context);

            await SeedUsers.SeedAsync(context);
            await SeedCategories.SeedAsync(context);
            await SeedProducts.SeedAsync(context);

            Console.WriteLine("Сидирование завершено успешно!");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка при сидировании: {e.Message}");
            throw;
        }
    }

    private static async Task DownAsync(AppDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" RESTART IDENTITY CASCADE;");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Categories\" RESTART IDENTITY CASCADE;");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Products\" RESTART IDENTITY CASCADE;");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ProductVariants\" RESTART IDENTITY CASCADE;");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Carts\" RESTART IDENTITY CASCADE;");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"CartItems\" RESTART IDENTITY CASCADE;");
    }
}