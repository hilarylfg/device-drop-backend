using System.Text.Json;
using device_drop_backend.Data;
using device_drop_backend.Models;

namespace device_drop_backend.Seed;

public static class SeedProducts
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var keyboard1 = new Product
            {
                Name = "Aula F75",
                Description = "Игровая механическая клавиатура с 75% форматом, переключателями LEOBOG Reaper, Gasket Mount строением, Hot-Swap системой, RGB-подсветкой и прочным корпусом. Подключение по радиоканалу, USB-C и Bluetooth.",
                Brand = "Aula",
                CategoryId = 1,
            };

            var keyboard2 = new Product
            {
                Name = "Wooting 60HE+",
                Description = "Игровая механическая клавиатура с 60% форматом, переключателями Lekker Magnetic с поддержкой регулировки точки срабатывания, технологией Rapid Trigger, RGB-подсветкой и прочным корпусом. Подключение по USB-C.",
                Brand = "Wooting",
                CategoryId = 1,
            };

            var keyboard5 = new Product
            {
                Name = "Keychron K6",
                Description = "Компактная механическая клавиатура с 65% форматом, переключателями Gateron, RGB-подсветкой и поддержкой Bluetooth.",
                Brand = "Keychron",
                CategoryId = 1,
            };

            var keyboard6 = new Product
            {
                Name = "MAD68 R",
                Description = "Игровая механическая клавиатура с 60% форматом, магнитными оптическими переключателями, Gasket Mount строением, Hot-Swap системой, RGB-подсветкой и прочным корпусом. Подключение по USB-C",
                Brand = "MAD",
                CategoryId = 1,
            };

            var keyboard3 = new Product
            {
                Name = "Razer Huntsman Mini",
                Description = "Компактная механическая клавиатура с 60% форматом, оптическими переключателями Razer и RGB-подсветкой.",
                Brand = "Razer",
                CategoryId = 1,
            };

            var keyboard4 = new Product
            {
                Name = "Logitech G Pro X",
                Description = "Игровая механическая клавиатура с заменяемыми переключателями, компактным дизайном и RGB-подсветкой.",
                Brand = "Logitech",
                CategoryId = 1,
            };

            var mouse1 = new Product
            {
                Name = "VXE MAD R Major +",
                Description = "Игровая мышь с сенсором PAW3950 и оптическим переключателем Huano Blue Shell Pink Dot. Вес 42г, частота опроса 8000 Гц, аккумулятор 500мАч. Подключение по радиоканалу и USB-C.",
                Brand = "VXE",
                CategoryId = 2,
            };

            var mouse2 = new Product
            {
                Name = "Logitech G Pro X Superlight",
                Description = "Игровая мышь с ультралегким дизайном (63 г), сенсором HERO 25K и беспроводным подключением.",
                Brand = "Logitech",
                CategoryId = 2,
            };

            var mouse3 = new Product
            {
                Name = "Razer DeathAdder V3 Pro",
                Description = "Игровая мышь с сенсором Focus Pro 30K, беспроводным подключением и эргономичным дизайном.",
                Brand = "Razer",
                CategoryId = 2,
            };

            var mouse4 = new Product
            {
                Name = "VGN Dragonfly F1 Moba",
                Description = "Игровая мышь с ультралегким дизайном, сенсором Pixart 3395 и RGB-подсветкой.",
                Brand = "Glorious",
                CategoryId = 2,
            };

            var mouse5 = new Product
            {
                Name = "SteelSeries Rival 3",
                Description = "Игровая мышь с сенсором TrueMove Core, RGB-подсветкой и прочным корпусом.",
                Brand = "SteelSeries",
                CategoryId = 2,
            };

            var headphones1 = new Product
            {
                Name = "SteelSeries Arctis Pro",
                Description = "Игровые наушники с высококачественным звуком, микрофоном с шумоподавлением и поддержкой Hi-Res Audio.",
                Brand = "SteelSeries",
                CategoryId = 3,
            };

            var headphones2 = new Product
            {
                Name = "HyperX Cloud II Wireless",
                Description = "Беспроводные игровые наушники с виртуальным 7.1 surround sound и микрофоном с шумоподавлением.",
                Brand = "HyperX",
                CategoryId = 3,
            };

            var headphones3 = new Product
            {
                Name = "Razer BlackShark V2",
                Description = "Игровые наушники с микрофоном с шумоподавлением и технологией THX 7.1 Surround Sound.",
                Brand = "Razer",
                CategoryId = 3,
            };

            var headphones4 = new Product
            {
                Name = "Sennheiser HD 599",
                Description = "Открытые наушники с высококачественным звуком и эргономичным дизайном.",
                Brand = "Sennheiser",
                CategoryId = 3,
            };

            var mousePad1 = new Product
            {
                Name = "SteelSeries QcK Heavy",
                Description = "Большой игровой коврик с толщиной 6 мм, тканевой поверхностью и резиновой основой.",
                Brand = "SteelSeries",
                CategoryId = 4,
            };

            var mousePad2 = new Product
            {
                Name = "Razer Gigantus V2",
                Description = "Игровой коврик с оптимизированной поверхностью для точного контроля и резиновой основой.",
                Brand = "Razer",
                CategoryId = 4,
            };

            var mousePad3 = new Product
            {
                Name = "Logitech G840",
                Description = "Большой игровой коврик с поверхностью, оптимизированной для точного контроля.",
                Brand = "Logitech",
                CategoryId = 4,
            };

            var mousePad4 = new Product
            {
                Name = "Corsair MM350",
                Description = "Игровой коврик с прочной тканью и резиновой основой для устойчивости.",
                Brand = "Corsair",
                CategoryId = 4,
            };

            var microphone1 = new Product
            {
                Name = "Blue Yeti",
                Description = "USB-микрофон с тремя конденсаторными капсюлями и поддержкой нескольких режимов записи.",
                Brand = "Blue",
                CategoryId = 5,
            };

            var microphone2 = new Product
            {
                Name = "Razer Seiren X",
                Description = "Компактный USB-микрофон с суперкардиоидной диаграммой направленности.",
                Brand = "Razer",
                CategoryId = 5,
            };

            var microphone3 = new Product
            {
                Name = "HyperX QuadCast",
                Description = "USB-микрофон с встроенным антивибрационным креплением и RGB-подсветкой.",
                Brand = "HyperX",
                CategoryId = 5,
            };

            var microphone4 = new Product
            {
                Name = "Audio-Technica AT2020",
                Description = "Конденсаторный микрофон с кардиоидной диаграммой направленности и высоким качеством звука.",
                Brand = "Audio-Technica",
                CategoryId = 5,
            };

            var accessory1 = new Product
            {
                Name = "Razer Mouse Bungee V3",
                Description = "Держатель для мыши, предотвращающий запутывание провода.",
                Brand = "Razer",
                CategoryId = 6,
            };

            var accessory2 = new Product
            {
                Name = "Logitech G Powerplay",
                Description = "Зарядная станция для беспроводных мышей Logitech G.",
                Brand = "Logitech",
                CategoryId = 6,
            };

            var products = new List<Product>
            {
                keyboard1, keyboard2, keyboard5, keyboard6, keyboard3, keyboard4,
                mouse1, mouse2, mouse3, mouse4, mouse5,
                headphones1, headphones2, headphones3, headphones4,
                mousePad1, mousePad2, mousePad3, mousePad4,
                microphone1, microphone2, microphone3, microphone4,
                accessory1, accessory2
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            var variants = new List<ProductVariant>
            {
                new ProductVariant { ProductId = keyboard1.Id, ColorId = 2, Price = 4990, SalePrice = 4399, Stock = 40, ImageUrl = "aula_f75_1.webp" }, // White
                new ProductVariant { ProductId = keyboard1.Id, ColorId = 12, Price = 5490, Stock = 10, ImageUrl = "aula_f75_2.webp" }, // Beige
                new ProductVariant { ProductId = keyboard1.Id, ColorId = 1, Price = 4790, Stock = 0, ImageUrl = "aula_f75_3.webp" }, // Black
                new ProductVariant { ProductId = keyboard2.Id, ColorId = 1, Price = 28990, Stock = 50, ImageUrl = "wooting_60he_1.webp" }, // Black
                new ProductVariant { ProductId = keyboard5.Id, ColorId = 3, Price = 8990, Stock = 30, ImageUrl = "keychron_k6_1.webp" }, // Grey
                new ProductVariant { ProductId = keyboard6.Id, ColorId = 1, Price = 6490, Stock = 20, ImageUrl = "mad68_r_1.webp" }, // Black
                new ProductVariant { ProductId = keyboard3.Id, ColorId = 1, Price = 14990, Stock = 15, ImageUrl = "razer_huntsman_mini_1.webp" }, // Black
                new ProductVariant { ProductId = keyboard4.Id, ColorId = 1, Price = 15990, Stock = 10, ImageUrl = "logitech_g_pro_x_keyboard_1.webp" }, // Black
                new ProductVariant { ProductId = mouse1.Id, ColorId = 2, Price = 6290, Stock = 30, ImageUrl = "vxe_major+_1.webp" }, // White
                new ProductVariant { ProductId = mouse1.Id, ColorId = 1, Price = 6290, SalePrice = 5999, Stock = 30, ImageUrl = "vxe_major+_2.webp" }, // Black
                new ProductVariant { ProductId = mouse2.Id, ColorId = 1, Price = 15990, Stock = 15, ImageUrl = "logitech_g_pro_x_superlight_1.webp" }, // Black
                new ProductVariant { ProductId = mouse3.Id, ColorId = 2, Price = 17990, Stock = 10, ImageUrl = "razer_deathadder_v3_pro_1.webp" }, // White
                new ProductVariant { ProductId = mouse4.Id, ColorId = 1, Price = 5490, Stock = 20, ImageUrl = "vgn_f1_moba_1.webp" }, // Black
                new ProductVariant { ProductId = mouse5.Id, ColorId = 1, Price = 4990, Stock = 30, ImageUrl = "steelseries_rival_3_1.webp" }, // Black
                new ProductVariant { ProductId = headphones1.Id, ColorId = 1, Price = 19990, Stock = 20, ImageUrl = "steelseries_arctis_pro_1.webp" }, // Black
                new ProductVariant { ProductId = headphones2.Id, ColorId = 4, Price = 14990, Stock = 25, ImageUrl = "hyperx_cloud_ii_wireless_1.webp" }, // Red
                new ProductVariant { ProductId = headphones3.Id, ColorId = 1, Price = 12990, Stock = 30, ImageUrl = "razer_blackshark_v2_1.webp" }, // Black
                new ProductVariant { ProductId = headphones4.Id, ColorId = 13, Price = 17990, Stock = 15, ImageUrl = "sennheiser_hd_599_1.webp" }, // Ivory
                new ProductVariant { ProductId = mousePad1.Id, ColorId = 1, Price = 2990, Stock = 50, ImageUrl = "steelseries_qck_heavy_1.webp" }, // Black
                new ProductVariant { ProductId = mousePad2.Id, ColorId = 1, Price = 3490, Stock = 40, ImageUrl = "razer_gigantus_v2_1.webp" }, // Black
                new ProductVariant { ProductId = mousePad3.Id, ColorId = 1, Price = 4990, Stock = 30, ImageUrl = "logitech_g840_1.webp" }, // Black
                new ProductVariant { ProductId = mousePad4.Id, ColorId = 1, Price = 3990, Stock = 35, ImageUrl = "corsair_mm350_1.webp" }, // Black
                new ProductVariant { ProductId = microphone1.Id, ColorId = 1, Price = 12990, Stock = 20, ImageUrl = "blue_yeti_1.webp" }, // Black
                new ProductVariant { ProductId = microphone2.Id, ColorId = 1, Price = 8990, Stock = 25, ImageUrl = "razer_seiren_x_1.webp" }, // Black
                new ProductVariant { ProductId = microphone3.Id, ColorId = 1, Price = 14990, Stock = 15, ImageUrl = "hyperx_quadcast_1.webp" }, // Black
                new ProductVariant { ProductId = microphone4.Id, ColorId = 1, Price = 9990, Stock = 30, ImageUrl = "audio_technica_at2020_1.webp" }, // Black
                new ProductVariant { ProductId = accessory1.Id, ColorId = 1, Price = 2990, Stock = 50, ImageUrl = "razer_mouse_bungee_v3_1.webp" }, // Black
                new ProductVariant { ProductId = accessory2.Id, ColorId = 1, Price = 14990, Stock = 10, ImageUrl = "logitech_g_powerplay_1.webp" } // Black
            };

            await context.ProductVariants.AddRangeAsync(variants);
            await context.SaveChangesAsync();
        }
    }