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

    [HttpGet("filtred")]
    public async Task<IActionResult> GetFiltredProducts(
        [FromQuery] string colors = null,
        [FromQuery] string brands = null,
        [FromQuery] bool availability = true,
        [FromQuery] bool discount = false,
        [FromQuery] decimal? priceFrom = null,
        [FromQuery] decimal? priceTo = null)
    {
        var colorIds = colors?.Split(',').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToList();
        var brandList = brands?.Split(',').Where(s => !string.IsNullOrEmpty(s)).ToList();

        var minPrice = priceFrom ?? 0;
        var maxPrice = priceTo ?? 1000000;

        var query = _context.Categories
            .Include(c => c.Products)
            .ThenInclude(p => p.Variants)
            .ThenInclude(v => v.Color)
            .AsQueryable();

        var categories = await query.ToListAsync();

        var result = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Link = c.Link,
            Products = c.Products
                .Where(p =>
                    (brandList == null || !brandList.Any() || brandList.Contains(p.Brand)) &&
                  
                    (colorIds == null || !colorIds.Any() || p.Variants.Any(v => colorIds.Contains(v.ColorId))) &&
                   
                    (!availability || p.Variants.Any(v => v.Stock > 0)) &&
                    
                    (!discount || p.Variants.Any(v => v.SalePrice.HasValue && v.SalePrice < v.Price)) &&
                    
                    p.Variants.Any(v => v.Price >= minPrice && v.Price <= maxPrice)
                )
                .OrderByDescending(p => p.Id)
                .Select(p => MapToProductDto(p))
                .ToList()
        }).ToList();

        result = result.Where(c => c.Products != null && c.Products.Any()).ToList();
        var totalCount = result.Sum(c => c.Products?.Count ?? 0);

        return Ok(new
        {
            Categories = result,
            TotalCount = totalCount,
        });
    }
    
    [HttpGet("colors")]
    public async Task<IActionResult> GetColors()
    {
        var colors = await _context.Colors
            .OrderBy(c => c.Id)
            .Select(c => new ColorDto
            {
                Id = c.Id,
                Hex = c.Hex,
                NameRu = c.NameRu,
                NameEn = c.NameEn
            })
            .ToListAsync();

        return Ok(colors);
    }

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands()
    {
        var brandsWithIds = await _context.Products
            .Where(p => !string.IsNullOrEmpty(p.Brand))
            .Select(p => new { p.Brand, p.Id })
            .Distinct()
            .OrderBy(x => x.Id)
            .ToListAsync();

        var uniqueBrands = brandsWithIds
            .GroupBy(x => x.Brand)
            .Select(g => g.OrderBy(x => x.Id).First())
            .OrderBy(x => x.Id)
            .Select(x => new { Id = x.Brand, Name = x.Brand })
            .ToList();

        return Ok(uniqueBrands);
    }

    private ProductDto MapToProductDto(Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Brand = p.Brand,
            Category = p.Category != null
                ? new CategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Link = p.Category.Link
                }
                : null,
            Variants = p.Variants?.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                ColorId = v.ColorId,
                Color = v.Color != null
                    ? new ColorDto
                    {
                        Id = v.Color.Id,
                        Hex = v.Color.Hex,
                        NameRu = v.Color.NameRu,
                        NameEn = v.Color.NameEn
                    }
                    : null,
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