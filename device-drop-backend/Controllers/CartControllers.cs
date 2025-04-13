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
        var cart = await GetCartByToken(token);
        return Ok(MapToCartDto(cart));
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] CreateCartItemDto dto)
    {
        var token = Request.Cookies["cartToken"] ?? Guid.NewGuid().ToString();
        var cart = await FindOrCreateCart(token);

        var productVariant = await _context.ProductVariants.FindAsync(dto.ProductVariantId);
        if (productVariant == null)
        {
            return NotFound(new { error = "Вариант продукта не найден" });
        }

        var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductVariantId == dto.ProductVariantId);
        if (cartItem != null)
        {
            cartItem.Quantity++;
        }
        else
        {
            cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductVariantId = dto.ProductVariantId,
                Quantity = 1,
            };
            _context.CartItems.Add(cartItem);
        }

        cart.TotalAmount = cart.Items.Sum(i => i.Quantity * (i.ProductVariant.SalePrice ?? i.ProductVariant.Price));
        await _context.SaveChangesAsync();

        SetCartCookie(token);
        return Ok(MapToCartDto(cart));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDto dto)
    {
        var token = Request.Cookies["cartToken"];
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { error = "Токен корзины не найден" });
        }

        var cart = await GetCartByToken(token);
        var cartItem = cart.Items.FirstOrDefault(ci => ci.Id == id);
        if (cartItem == null)
        {
            return NotFound(new { error = "Товар не найден в корзине" });
        }

        if (dto.Quantity <= 0)
        {
            _context.CartItems.Remove(cartItem);
        }
        else
        {
            cartItem.Quantity = dto.Quantity;
        }

        cart.TotalAmount = cart.Items.Sum(i => i.Quantity * (i.ProductVariant.SalePrice ?? i.ProductVariant.Price));
        await _context.SaveChangesAsync();

        return Ok(MapToCartDto(cart));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCartItem(int id)
    {
        var token = Request.Cookies["cartToken"];
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { error = "Токен корзины не найден" });
        }

        var cart = await GetCartByToken(token);
        var cartItem = cart.Items.FirstOrDefault(ci => ci.Id == id);
        if (cartItem == null)
        {
            return NotFound(new { error = "Товар не найден в корзине" });
        }

        _context.CartItems.Remove(cartItem);
        cart.TotalAmount = cart.Items.Sum(i => i.Quantity * (i.ProductVariant.SalePrice ?? i.ProductVariant.Price));
        await _context.SaveChangesAsync();

        return Ok(MapToCartDto(cart));
    }

    private async Task<Cart> FindOrCreateCart(string token)
    {
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Token == token);
        if (cart != null)
        {
            return cart;
        }

        cart = new Cart { Token = token, TotalAmount = 0, Items = [] };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    private async Task<Cart> GetCartByToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return new Cart { TotalAmount = 0, Items = [] };
        }

        var cart = await _context.Carts
            .Include(c => c.Items.OrderByDescending(i => i.CreatedAt))  
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(c => c.Token == token);

        return cart ?? new Cart { TotalAmount = 0, Items = [] };
    }

    private void SetCartCookie(string token)
    {
        Response.Cookies.Append("cartToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,  
            MaxAge = TimeSpan.FromDays(2),
            Path = "/",
            SameSite = SameSiteMode.Lax
        });
    }

    private CartDto MapToCartDto(Cart cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            TotalAmount = cart.TotalAmount,
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                Quantity = i.Quantity,
                ProductVariant = new ProductVariantDto
                {
                    Id = i.ProductVariant.Id,
                    Price = i.ProductVariant.Price,
                    SalePrice = i.ProductVariant.SalePrice,
                    Stock = i.ProductVariant.Stock,
                    ImageUrl = i.ProductVariant.ImageUrl ?? string.Empty,
                    ColorId = i.ProductVariant.ColorId,
                    Product = i.ProductVariant.Product != null ? new ProductDto
                    {
                        Id = i.ProductVariant.Product.Id,
                        Name = i.ProductVariant.Product.Name ?? string.Empty,
                        Description = i.ProductVariant.Product.Description ?? string.Empty,
                        Brand = i.ProductVariant.Product.Brand ?? string.Empty,
                        CategoryId = i.ProductVariant.Product.CategoryId
                    } : null
                }
            }).ToList()
        };
    }
}