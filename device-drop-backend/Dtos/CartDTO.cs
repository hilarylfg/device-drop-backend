namespace device_drop_backend.Dtos;

public class CreateCartItemDto
{
    public int ProductVariantId { get; set; }
}

public class UpdateCartItemDto
{
    public int Quantity { get; set; }
}

public class CartDto
{
    public int Id { get; set; }
    public int TotalAmount { get; set; }
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
}

public class CartItemDto
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public ProductVariantDtoForCart ProductVariant { get; set; } = null!;
}

public class ProductDtoForCart
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}

public class ProductVariantDtoForCart
{
    public int Id { get; set; }
    public int Price { get; set; }
    public int? SalePrice { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int ColorId { get; set; }
    public ProductDtoForCart Product { get; set; } = null!;
}