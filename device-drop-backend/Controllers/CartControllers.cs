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
                    ProductVariant = new ProductVariantDtoForCart
                    {
                        Id = i.ProductVariant.Id,
                        Price = i.ProductVariant.Price,
                        SalePrice = i.ProductVariant.SalePrice,
                        Stock = i.ProductVariant.Stock,
                        ImageUrl = i.ProductVariant.ImageUrl,
                        ColorId = i.ProductVariant.ColorId,
                        Product = new ProductDtoForCart
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

            return Ok(cartDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CART_GET] Server error: {ex.Message}");
            return StatusCode(500, new { message = "Не удалось получить корзину" });
        }
    }

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
                    ProductVariant = new ProductVariantDtoForCart
                    {
                        Id = i.ProductVariant.Id,
                        Price = i.ProductVariant.Price,
                        SalePrice = i.ProductVariant.SalePrice,
                        Stock = i.ProductVariant.Stock,
                        ImageUrl = i.ProductVariant.ImageUrl,
                        ColorId = i.ProductVariant.ColorId,
                        Product = new ProductDtoForCart
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

            var response = Ok(cartDto);
            if (string.IsNullOrEmpty(Request.Cookies["cartToken"]))
            {
                Response.Cookies.Append("cartToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, 
                    MaxAge = TimeSpan.FromDays(2),
                    Path = "/",
                    SameSite = SameSiteMode.Lax,
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
                    ProductVariant = new ProductVariantDtoForCart
                    {
                        Id = i.ProductVariant.Id,
                        Price = i.ProductVariant.Price,
                        SalePrice = i.ProductVariant.SalePrice,
                        Stock = i.ProductVariant.Stock,
                        ImageUrl = i.ProductVariant.ImageUrl,
                        ColorId = i.ProductVariant.ColorId,
                        Product = new ProductDtoForCart
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
                    ProductVariant = new ProductVariantDtoForCart
                    {
                        Id = i.ProductVariant.Id,
                        Price = i.ProductVariant.Price,
                        SalePrice = i.ProductVariant.SalePrice,
                        Stock = i.ProductVariant.Stock,
                        ImageUrl = i.ProductVariant.ImageUrl,
                        ColorId = i.ProductVariant.ColorId,
                        Product = new ProductDtoForCart
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
        var userCart = await _context.Carts
            .FirstOrDefaultAsync(c => c.Token == token);

        if (userCart == null)
        {
            userCart = new Cart { Token = token };
            _context.Carts.Add(userCart);
            await _context.SaveChangesAsync();
        }

        return userCart;
    }

    private async Task<Cart> UpdateCartTotalAmount(string token)
    {
        var userCart = await _context.Carts
            .Include(c => c.Items.OrderByDescending(i => i.CreatedAt))
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(c => c.Token == token);

        if (userCart == null)
        {
            return null;
        }

        var totalAmount = userCart.Items.Sum(i => CalcCartItemTotalPrice(i));

        userCart.TotalAmount = totalAmount;
        _context.Carts.Update(userCart);
        await _context.SaveChangesAsync();

        return await _context.Carts
            .Include(c => c.Items.OrderByDescending(i => i.CreatedAt))
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(c => c.Token == token);
        
    }
    
    private int CalcCartItemTotalPrice(CartItem item)
    {
        return item.Quantity * (item.ProductVariant.SalePrice ?? item.ProductVariant.Price);
    }
}