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
    public async Task<IActionResult> AddToCart([FromBody] CreateCartItemDto data)
    {
        var token = Request.Cookies["cartToken"] ?? Guid.NewGuid().ToString();
        var cart = await FindOrCreateCart(token);

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductVariantId == data.ProductVariantId);

        if (cartItem != null)
        {
            cartItem.Quantity++;
        }
        else
        {
            cartItem = new CartItem { CartId = cart.Id, ProductVariantId = data.ProductVariantId, Quantity = 1 };
            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();
        var updatedCart = await UpdateCartTotalAmount(token);

        if (string.IsNullOrEmpty(Request.Cookies["cartToken"]))
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

        return Ok(MapToCartDto(updatedCart));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDto data)
    {
        var token = Request.Cookies["cartToken"];
        if (string.IsNullOrEmpty(token)) return BadRequest(new { error = "Cart token not found" });

        var cartItem = await _context.CartItems.FindAsync(id);
        if (cartItem == null) return NotFound(new { error = "Cart item not found" });

        cartItem.Quantity = data.Quantity;
        await _context.SaveChangesAsync();
        var updatedCart = await UpdateCartTotalAmount(token);

        return Ok(MapToCartDto(updatedCart));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCartItem(int id)
    {
        var token = Request.Cookies["cartToken"];
        if (string.IsNullOrEmpty(token)) return BadRequest(new { error = "Cart token not found" });

        var cartItem = await _context.CartItems.FindAsync(id);
        if (cartItem == null) return NotFound(new { error = "Cart item not found" });

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
        var updatedCart = await UpdateCartTotalAmount(token);

        return Ok(MapToCartDto(updatedCart));
    }

    private async Task<Cart> FindOrCreateCart(string token)
    {
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Token == token);
        if (cart == null)
        {
            cart = new Cart { Token = token };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    private async Task<Cart> UpdateCartTotalAmount(string token)
    {
        var cart = await GetCartByToken(token);
        if (cart == null) return null;

        cart.TotalAmount = cart.Items.Sum(i => i.Quantity * (i.ProductVariant.SalePrice ?? i.ProductVariant.Price));
        await _context.SaveChangesAsync();
        return await GetCartByToken(token);
    }

    private async Task<Cart> GetCartByToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return new Cart { TotalAmount = 0, Items = [] };

        return await _context.Carts
            .Include(c => c.Items.OrderByDescending(i => i.CreatedAt))
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(pv => pv.Product) 
            .FirstOrDefaultAsync(c => c.Token == token) ?? new Cart { TotalAmount = 0, Items = [] };
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
                    ImageUrl = i.ProductVariant.ImageUrl,
                    ColorId = i.ProductVariant.ColorId,
           
                    Product = new ProductDto
                    {
                        Id = i.ProductVariant.Product.Id,
                        Name = i.ProductVariant.Product.Name,
                        Description = i.ProductVariant.Product.Description,
                        Brand = i.ProductVariant.Product.Brand,
                        CategoryId = i.ProductVariant.Product.CategoryId
                    }
                }
            }).ToList()
        };
    }
}   