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
        var cart  = await GetCartByToken(token);
        return Ok(MapToCartDto(cart));
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] CreateCartItemDto dto)
    {
        var token = Request.Cookies["cartToken"] ?? Guid.NewGuid().ToString();
        var cart  = await FindOrCreateCart(token);

        var productVariant = await _context.ProductVariants
            .Include(pv => pv.Product)
            .FirstOrDefaultAsync(pv => pv.Id == dto.ProductVariantId);

        if (productVariant == null)
            return NotFound(new { error = "Вариант продукта не найден" });

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci =>
                ci.CartId == cart.Id && ci.ProductVariantId == dto.ProductVariantId);

        if (cartItem != null)
        {
            cartItem.Quantity++;
        }
        else
        {
            cartItem = new CartItem
            {
                CartId          = cart.Id,
                ProductVariant  = productVariant,
                ProductVariantId= dto.ProductVariantId,
                Quantity        = 1
            };
            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();
        await RecalculateTotalAmountAsync(cart.Id);

        var updatedCart = await GetCartByToken(token);
        SetCartCookie(token);
        return Ok(MapToCartDto(updatedCart));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDto dto)
    {
        var token = Request.Cookies["cartToken"];
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { error = "Токен корзины не найден" });

        var cartItem = await _context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.Token == token);

        if (cartItem == null)
            return NotFound(new { error = "Товар не найден в корзине" });

        if (dto.Quantity <= 0)
            _context.CartItems.Remove(cartItem);
        else
            cartItem.Quantity = dto.Quantity;

        await _context.SaveChangesAsync();
        await RecalculateTotalAmountAsync(cartItem.CartId);

        var updatedCart = await GetCartByToken(token);
        return Ok(MapToCartDto(updatedCart));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCartItem(int id)
    {
        var token = Request.Cookies["cartToken"];
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { error = "Токен корзины не найден" });

        var cartItem = await _context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.Token == token);

        if (cartItem == null)
            return NotFound(new { error = "Товар не найден в корзине" });

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
        await RecalculateTotalAmountAsync(cartItem.CartId);

        var updatedCart = await GetCartByToken(token);
        return Ok(MapToCartDto(updatedCart));
    }

    private async Task<Cart> FindOrCreateCart(string token)
    {
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Token == token);
        if (cart != null) return cart;

        cart = new Cart { Token = token, TotalAmount = 0, Items = [] };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    private async Task<Cart> GetCartByToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return new Cart { TotalAmount = 0, Items = [] };

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
            // Secure   = true,
            MaxAge   = TimeSpan.FromDays(2),
            Path     = "/",
            // SameSite = SameSiteMode.Lax
        });
    }

    private async Task RecalculateTotalAmountAsync(int cartId)
    {
        var cartItems = await _context.CartItems
            .Include(ci => ci.ProductVariant)
            .Where(ci => ci.CartId == cartId)
            .ToListAsync();

        var total = cartItems
            .Sum(ci => ci.Quantity * (ci.ProductVariant.SalePrice ?? ci.ProductVariant.Price));

        var cart = await _context.Carts.FirstAsync(c => c.Id == cartId);
        cart.TotalAmount = total;
        await _context.SaveChangesAsync();
    }
    
    private CartDto MapToCartDto(Cart cart) => new CartDto
    {
        Id          = cart.Id,
        TotalAmount = cart.TotalAmount,
        Items       = cart.Items.Select(i => new CartItemDto
        {
            Id       = i.Id,
            Quantity = i.Quantity,
            ProductVariant = new ProductVariantDto
            {
                Id        = i.ProductVariant.Id,
                Price     = i.ProductVariant.Price,
                SalePrice = i.ProductVariant.SalePrice,
                Stock     = i.ProductVariant.Stock,
                ImageUrl  = i.ProductVariant.ImageUrl ?? string.Empty,
                ColorId   = i.ProductVariant.ColorId,
                Product   = i.ProductVariant.Product != null ? new ProductDto
                {
                    Id          = i.ProductVariant.Product.Id,
                    Name        = i.ProductVariant.Product.Name ?? string.Empty,
                    Description = i.ProductVariant.Product.Description ?? string.Empty,
                    Brand       = i.ProductVariant.Product.Brand ?? string.Empty,
                    CategoryId  = i.ProductVariant.Product.CategoryId
                } : null
            }
        }).ToList()
    };
}