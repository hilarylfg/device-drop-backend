using device_drop_backend.Data;
using device_drop_backend.Dtos;
using device_drop_backend.Models;
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
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? NotFound() : Ok(MapToProductDto(product));
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string query = "")
    {
        var products = await _context.Products
            .Include(p => p.Variants)
            .ThenInclude(v => v.Color)
            .Where(p => EF.Functions.ILike(p.Name, $"%{query}%"))
            .OrderBy(p => p.Name)
            .Take(5)
            .ToListAsync();

        var productDtos = products.Select(MapToProductDto).ToList();
        return Ok(productDtos);
    }

    [HttpGet("similar/{id}")]
    public async Task<IActionResult> GetSimilarProducts(int id)
    {
        var currentProduct = await _context.Products
            .Include(p => p.Variants)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (currentProduct == null) return NotFound(new { error = "Product not found" });
        if (!currentProduct.Variants.Any()) return BadRequest(new { error = "Product has no variants" });

        var basePrice = currentProduct.Variants.Min(v => v.Price);
        const int priceRange = 2000;

        var similarProducts = await _context.Products
            .Include(p => p.Variants)
            .ThenInclude(v => v.Color)
            .Where(p => p.Id != id
                        && p.CategoryId == currentProduct.CategoryId
                        && p.Variants.Any(v => v.Price >= basePrice - priceRange && v.Price <= basePrice + priceRange))
            .OrderBy(p => p.Variants.Min(v => v.Price))
            .Take(4)
            .ToListAsync();

        int needed = 4 - similarProducts.Count;
        if (needed > 0)
        {
            var additionalProducts = await _context.Products
                .Include(p => p.Variants)
                .ThenInclude(v => v.Color)
                .Where(p => p.Id != id
                            && p.CategoryId == currentProduct.CategoryId
                            && !similarProducts.Select(sp => sp.Id).Contains(p.Id))
                .OrderBy(p => p.Variants.Min(v => v.Price))
                .Take(needed)
                .ToListAsync();

            similarProducts.AddRange(additionalProducts);
        }

        var similarProductDtos = similarProducts.Select(MapToProductDtoWithMinimalVariant).ToList();
        return Ok(similarProductDtos);
    }

    [HttpGet("length")]
    public async Task<IActionResult> GetProductsLength()
    {
        return Ok(await _context.Products.CountAsync());
    }

    private ProductDto MapToProductDto(Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Brand = p.Brand,
            Category = p.Category != null ? new CategoryDto
            {
                Id = p.Category.Id,
                Name = p.Category.Name,
                Link = p.Category.Link
            } : null,
            Variants = p.Variants?.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                ColorId = v.ColorId,
                Color = v.Color != null ? new ColorDto
                {
                    Id = v.Color.Id,
                    Hex = v.Color.Hex,
                    NameRu = v.Color.NameRu,
                    NameEn = v.Color.NameEn
                } : null,
                Price = v.Price,
                SalePrice = v.SalePrice,
                Stock = v.Stock,
                ImageUrl = v.ImageUrl
            }).ToList() ?? []
        };
    }

    private ProductDto MapToProductDtoWithMinimalVariant(Product p)
    {
        return new ProductDto
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
        };
    }
}