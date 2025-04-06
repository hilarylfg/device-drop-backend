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
        try
        {
            string token = Request.Cookies["cartToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Ok(new CartDto { TotalAmount = 0, Items = new List<CartItemDto>() });
            }

            var userCart = await _context.Carts
                .Include(c => c.Items.OrderByDescending(i => i.CreatedAt))
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(c => c.Token == token);

            if (userCart == null)
            {
                return Ok(new CartDto { TotalAmount = 0, Items = new List<CartItemDto>() });
            }

            var cartDto = new CartDto
            {
                Id = userCart.Id,
                TotalAmount = userCart.TotalAmount,
                Items = userCart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    Quantity = i.Quantity,
                    ProductVariant = new ProductVariantDto
                    {
                        Id = i.ProductVariant.Id,
                        Price = i.ProductVariant.Price,
                        ImageUrl = i.ProductVariant.ImageUrl
                    }
                }).ToList()
            };

            return Ok(cartDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CART_GET] Server error: {ex.Message}");
            return StatusCode(500, new { message = "Не удалось получить корзину" });
        }
    }

    // POST: api/cart
    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] CreateCartItemDto data)
    {
        try
        {
            string token = Request.Cookies["cartToken"];
            if (string.IsNullOrEmpty(token))
            {
                token = Guid.NewGuid().ToString();
            }

            var userCart = await FindOrCreateCart(token);

            var findCartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == userCart.Id && ci.ProductVariantId == data.ProductVariantId);

            if (findCartItem != null)
            {
                findCartItem.Quantity += 1;
                await _context.SaveChangesAsync();
            }
            else
            {
                var newCartItem = new CartItem
                {
                    CartId = userCart.Id,
                    ProductVariantId = data.ProductVariantId,
                    Quantity = 1
                };
                _context.CartItems.Add(newCartItem);
                await _context.SaveChangesAsync();
            }

            await UpdateCartTotalAmount(token);

            var updatedCart = await _context.Carts
                .Include(c => c.Items.OrderByDescending(i => i.CreatedAt))
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(c => c.Token == token);

            var cartDto = new CartDto
            {
                Id = updatedCart.Id,
                TotalAmount = updatedCart.TotalAmount,
                Items = updatedCart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    Quantity = i.Quantity,
                    ProductVariant = new ProductVariantDto
                    {
                        Id = i.ProductVariant.Id,
                        Price = i.ProductVariant.Price,
                        ImageUrl = i.ProductVariant.ImageUrl
                    }
                }).ToList()
            };

            var response = Ok(cartDto);
            if (Request.Cookies["cartToken"] == null)
            {
                Response.Cookies.Append("cartToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Используйте true, если используете HTTPS
                    MaxAge = TimeSpan.FromDays(2),
                    Path = "/",
                    SameSite = SameSiteMode.Strict
                });
            }
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CART_POST] Server error: {ex.Message}");
            return StatusCode(500, new { message = "Не удалось создать корзину" });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDto data)
    {
        try
        {
            string token = Request.Cookies["cartToken"];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { error = "Cart token not found" });
            }

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.Id == id);
            if (cartItem == null)
            {
                return NotFound(new { error = "Cart item not found" });
            }

            cartItem.Quantity = data.Quantity;
            await _context.SaveChangesAsync();

            await UpdateCartTotalAmount(token);

            var updatedCart = await _context.Carts
                .Include(c => c.Items.OrderByDescending(i => i.CreatedAt))
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(c => c.Token == token);

            var cartDto = new CartDto
            {
                Id = updatedCart.Id,
                TotalAmount = updatedCart.TotalAmount,
                Items = updatedCart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    Quantity = i.Quantity,
                    ProductVariant = new ProductVariantDto
                    {
                        Id = i.ProductVariant.Id,
                        Price = i.ProductVariant.Price,
                        ImageUrl = i.ProductVariant.ImageUrl
                    }
                }).ToList()
            };

            return Ok(cartDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CART_PATCH] Server error: {ex.Message}");
            return StatusCode(500, new { message = "Не удалось обновить корзину" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCartItem(int id)
    {
        try
        {
            string token = Request.Cookies["cartToken"];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { error = "Cart token not found" });
            }

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.Id == id);
            if (cartItem == null)
            {
                return NotFound(new { error = "Cart item not found" });
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            await UpdateCartTotalAmount(token);

            var updatedCart = await _context.Carts
                .Include(c => c.Items.OrderByDescending(i => i.CreatedAt))
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(c => c.Token == token);

            var cartDto = new CartDto
            {
                Id = updatedCart.Id,
                TotalAmount = updatedCart.TotalAmount,
                Items = updatedCart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    Quantity = i.Quantity,
                    ProductVariant = new ProductVariantDto
                    {
                        Id = i.ProductVariant.Id,
                        Price = i.ProductVariant.Price,
                        ImageUrl = i.ProductVariant.ImageUrl
                    }
                }).ToList()
            };

            return Ok(cartDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CART_DELETE] Server error: {ex.Message}");
            return StatusCode(500, new { message = "Не удалось удалить корзину" });
        }
    }

    private async Task<Cart> FindOrCreateCart(string token)
    {
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Token == token);
        if (cart == null)
        {
            cart = new Cart { Token = token, TotalAmount = 0 };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }
        return cart;
    }

    private async Task UpdateCartTotalAmount(string token)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.Token == token);

        if (cart != null)
        {
            cart.TotalAmount = cart.Items.Sum(i => i.Quantity * i.ProductVariant.Price);
            await _context.SaveChangesAsync();
        }
    }
}