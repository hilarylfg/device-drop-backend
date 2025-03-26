using device_drop_backend.Data;
using device_drop_backend.Models;

namespace device_drop_backend.Seed;

public static class SeedCategories
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var categories = new List<Category>
        {
            new Category { Name = "Клавиатуры", Link = "keyboards" },
            new Category { Name = "Мышки", Link = "mouses" },
            new Category { Name = "Наушники", Link = "headphones" },
            new Category { Name = "Коврики", Link = "pads" },
            new Category { Name = "Микрофоны", Link = "microphones" },
            new Category { Name = "Аксессуары", Link = "accessories" }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}