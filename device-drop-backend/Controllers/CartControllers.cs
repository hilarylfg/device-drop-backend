using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using device_drop_backend.Data;
using device_drop_backend.Dtos;
using device_drop_backend.Models;

[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var token = Request.Cookies["cartToken"];
        var dto = await BuildCartDto(token);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] CreateCartItemDto dto)
    {
        // 1) Получаем или создаём корзину
        var token = Request.Cookies["cartToken"] ?? Guid.NewGuid().ToString();
        var cart = await FindOrCreateCart(token);

        // 2) Добавляем/увеличиваем
        var existing = cart.Items
            .FirstOrDefault(ci => ci.ProductVariantId == dto.ProductVariantId);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            _context.CartItems.Add(new CartItem {
                CartId = cart.Id,
                ProductVariantId = dto.ProductVariantId,
                Quantity = 1,
            });
        }

        await _context.SaveChangesAsync();

        // 3) Вновь строим DTO из чистого запроса (чтобы не было рассинхронизации навигаций)
        var resultDto = await BuildCartDto(token);

        SetCartCookie(token);
        return Ok(resultDto);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDto dto)
    {
        var token = Request.Cookies["cartToken"];
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { error = "Токен корзины не найден" });

        var cart = await GetCartByToken(token);
        var item = cart.Items.FirstOrDefault(ci => ci.Id == id);
        if (item == null)
            return NotFound(new { error = "Товар не найден в корзине" });

        if (dto.Quantity <= 0)
            _context.CartItems.Remove(item);
        else
            item.Quantity = dto.Quantity;

        await _context.SaveChangesAsync();

        var resultDto = await BuildCartDto(token);
        return Ok(resultDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCartItem(int id)
    {
        var token = Request.Cookies["cartToken"];
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { error = "Токен корзины не найден" });

        var cart = await GetCartByToken(token);
        var item = cart.Items.FirstOrDefault(ci => ci.Id == id);
        if (item == null)
            return NotFound(new { error = "Товар не найден в корзине" });

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();

        var resultDto = await BuildCartDto(token);
        return Ok(resultDto);
    }

    private async Task<Cart> GetCartByToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return new Cart { TotalAmount = 0, Items = new List<CartItem>() };

        var cart = await _context.Carts
            .Include(c => c.Items.OrderByDescending(i => i.CreatedAt))
              .ThenInclude(i => i.ProductVariant)
                .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(c => c.Token == token);

        return cart ?? new Cart { TotalAmount = 0, Items = new List<CartItem>() };
    }

    // Вспомогательный: создаёт DTO, пересчитывая TotalAmount «с нуля»
    private async Task<CartDto> BuildCartDto(string token)
    {
        var cart = await GetCartByToken(token);

        // Пересчитываем TotalAmount на основе актуальной коллекции
        cart.TotalAmount = cart.Items
            .Sum(i => i.Quantity * (i.ProductVariant.SalePrice ?? i.ProductVariant.Price));

        return new CartDto {
            Id = cart.Id,
            TotalAmount = cart.TotalAmount,
            Items = cart.Items.Select(i => new CartItemDto {
                Id = i.Id,
                Quantity = i.Quantity,
                ProductVariant = new ProductVariantDto {
                    Id = i.ProductVariant.Id,
                    Price = i.ProductVariant.Price,
                    SalePrice = i.ProductVariant.SalePrice,
                    Stock = i.ProductVariant.Stock,
                    ImageUrl = i.ProductVariant.ImageUrl ?? string.Empty,
                    ColorId = i.ProductVariant.ColorId,
                    Product = i.ProductVariant.Product is null ? null : new ProductDto {
                        Id = i.ProductVariant.Product.Id,
                        Name = i.ProductVariant.Product.Name!,
                        Description = i.ProductVariant.Product.Description!,
                        Brand = i.ProductVariant.Product.Brand!,
                        CategoryId = i.ProductVariant.Product.CategoryId
                    }
                }
            }).ToList()
        };
    }

    private async Task<Cart> FindOrCreateCart(string token)
    {
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Token == token);
        if (cart != null)
            return cart;

        cart = new Cart {
            Token = token,
            TotalAmount = 0,
            Items = new List<CartItem>()
        };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    private void SetCartCookie(string token)
    {
        Response.Cookies.Append("cartToken", token, new CookieOptions {
            HttpOnly = true,
            Secure = false,
            MaxAge = TimeSpan.FromDays(2),
            Path = "/",
            SameSite = SameSiteMode.Lax
        });
    }
}