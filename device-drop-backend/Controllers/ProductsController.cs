using device_drop_backend.Data;
using device_drop_backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace device_drop_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _context.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                Category = new CategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Link = p.Category.Link
                },
                Variants = p.Variants.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    ColorId = v.ColorId,
                    Color = new ColorDto
                    {
                        Id = v.Color.Id,
                        Hex = v.Color.Hex,
                        NameRu = v.Color.NameRu,
                        NameEn = v.Color.NameEn
                    },
                    Price = v.Price,
                    SalePrice = v.SalePrice,
                    Stock = v.Stock,
                    ImageUrl = v.ImageUrl
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string query = "")
    {
        var products = await _context.Products
            .Where(p => EF.Functions.ILike(p.Name, $"%{query}%"))
            .OrderBy(p => p.Name)
            .Take(5)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                Variants = p.Variants.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    ColorId = v.ColorId,
                    Color = new ColorDto
                    {
                        Id = v.Color.Id,
                        Hex = v.Color.Hex,
                        NameRu = v.Color.NameRu,
                        NameEn = v.Color.NameEn
                    },
                    Price = v.Price,
                    SalePrice = v.SalePrice,
                    Stock = v.Stock,
                    ImageUrl = v.ImageUrl
                }).ToList()
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("similar/{id}")]
    public async Task<IActionResult> GetSimilarProducts(int id)
    {
        try
        {
            var currentProduct = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (currentProduct == null)
            {
                return NotFound(new { error = "Product not found" });
            }

            if (!currentProduct.Variants.Any())
            {
                return BadRequest(new { error = "Product has no variants" });
            }

            var basePrice = currentProduct.Variants.OrderBy(v => v.Price).First().Price;
            const int priceRange = 2000;

            var similarProducts = await _context.Products
                .Where(p => p.Id != id
                            && p.CategoryId == currentProduct.CategoryId
                            && p.Variants.Any(v =>
                                v.Price >= basePrice - priceRange && v.Price <= basePrice + priceRange))
                .OrderBy(p => p.Variants.Min(v => v.Price)) // Сортировка по минимальной цене варианта
                .Take(4)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Brand = p.Brand,
                    CategoryId = p.CategoryId,
                    Variants = p.Variants
                        .OrderBy(v => v.Price)
                        .Take(1)
                        .Select(v => new ProductVariantDto
                        {
                            Id = v.Id,
                            ColorId = v.ColorId,
                            Color = new ColorDto
                            {
                                Id = v.Color.Id,
                                Hex = v.Color.Hex,
                                NameRu = v.Color.NameRu,
                                NameEn = v.Color.NameEn
                            },
                            Price = v.Price,
                            SalePrice = v.SalePrice,
                            Stock = v.Stock,
                            ImageUrl = v.ImageUrl
                        }).ToList()
                })
                .ToListAsync();

            int similarCount = similarProducts.Count;

            if (similarCount < 4)
            {
                var additionalProducts = await _context.Products
                    .Where(p => p.Id != id && p.CategoryId == currentProduct.CategoryId
                                           && !similarProducts.Select(sp => sp.Id).Contains(p.Id))
                    .OrderBy(p => p.Variants.Min(v => v.Price)) // Сортировка по минимальной цене
                    .Take(4 - similarCount)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Brand = p.Brand,
                        CategoryId = p.CategoryId,
                        Variants = p.Variants
                            .OrderBy(v => v.Price)
                            .Take(1)
                            .Select(v => new ProductVariantDto
                            {
                                Id = v.Id,
                                ColorId = v.ColorId,
                                Color = new ColorDto
                                {
                                    Id = v.Color.Id,
                                    Hex = v.Color.Hex,
                                    NameRu = v.Color.NameRu,
                                    NameEn = v.Color.NameEn
                                },
                                Price = v.Price,
                                SalePrice = v.SalePrice,
                                Stock = v.Stock,
                                ImageUrl = v.ImageUrl
                            }).ToList()
                    })
                    .ToListAsync();

                similarProducts.AddRange(additionalProducts);

                if (similarProducts.Count < 4)
                {
                    var extraNeeded = 4 - similarProducts.Count;
                    var extraProducts = await _context.Products
                        .Where(p => p.CategoryId == currentProduct.CategoryId
                                    && !similarProducts.Select(sp => sp.Id).Contains(p.Id))
                        .OrderBy(p => p.Variants.Min(v => v.Price)) // Сортировка по минимальной цене
                        .Take(extraNeeded)
                        .Select(p => new ProductDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Description = p.Description,
                            Brand = p.Brand,
                            CategoryId = p.CategoryId,
                            Variants = p.Variants
                                .OrderBy(v => v.Price)
                                .Take(1)
                                .Select(v => new ProductVariantDto
                                {
                                    Id = v.Id,
                                    ColorId = v.ColorId,
                                    Color = new ColorDto
                                    {
                                        Id = v.Color.Id,
                                        Hex = v.Color.Hex,
                                        NameRu = v.Color.NameRu,
                                        NameEn = v.Color.NameEn
                                    },
                                    Price = v.Price,
                                    SalePrice = v.SalePrice,
                                    Stock = v.Stock,
                                    ImageUrl = v.ImageUrl
                                }).ToList()
                        })
                        .ToListAsync();

                    similarProducts.AddRange(extraProducts);
                }
            }

            return Ok(similarProducts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching similar products: {ex.Message}");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("length")]
    public async Task<IActionResult> GetProductsLength()
    {
        var count = await _context.Products.CountAsync();
        return Ok(count);
    }
}