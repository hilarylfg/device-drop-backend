using device_drop_backend.Data;
using device_drop_backend.Dtos;
using device_drop_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace device_drop_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        return Ok(await _context.Categories.ToListAsync());
    }

    [HttpGet("with-products")]
    public async Task<IActionResult> GetCategoriesWithProducts()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .ThenInclude(p => p.Variants)
            .ThenInclude(v => v.Color)
            .ToListAsync();

        var categoryDtos = categories.Select(MapToCategoryDto).ToList();
        return Ok(categoryDtos);
    }

    private CategoryDto MapToCategoryDto(Category c)
    {
        return new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Link = c.Link,
            Products = c.Products.Select(p => new ProductDto
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
            }).ToList()
        };
    }
}