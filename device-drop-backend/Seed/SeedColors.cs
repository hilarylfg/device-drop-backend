using device_drop_backend.Data;
using device_drop_backend.Models;

namespace device_drop_backend.Seed;

public static class SeedColors
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var colors = new List<Color>
        {
            new Color { Hex = "#000000", NameRu = "Черный", NameEn = "Black" },
            new Color { Hex = "#ffffff", NameRu = "Белый", NameEn = "White" },
            new Color { Hex = "#808080", NameRu = "Серый", NameEn = "Grey" },
            new Color { Hex = "#ff0000", NameRu = "Красный", NameEn = "Red" },
            new Color { Hex = "#0000ff", NameRu = "Синий", NameEn = "Blue" },
            new Color { Hex = "#008000", NameRu = "Зеленый", NameEn = "Green" },
            new Color { Hex = "#ffff00", NameRu = "Желтый", NameEn = "Yellow" },
            new Color { Hex = "#ffa500", NameRu = "Оранжевый", NameEn = "Orange" },
            new Color { Hex = "#800080", NameRu = "Фиолетовый", NameEn = "Purple" },
            new Color { Hex = "#ffc0cb", NameRu = "Розовый", NameEn = "Pink" },
            new Color { Hex = "#a52a2a", NameRu = "Коричневый", NameEn = "Brown" },
            new Color { Hex = "#f5f5dc", NameRu = "Бежевый", NameEn = "Beige" },
            new Color { Hex = "#ffffe6", NameRu = "Слоновая кость", NameEn = "Ivory" }
        };

        await context.Colors.AddRangeAsync(colors);
        await context.SaveChangesAsync();
    }
}