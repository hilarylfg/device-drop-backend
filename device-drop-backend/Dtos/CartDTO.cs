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
    public ProductVariantDto ProductVariant { get; set; }
}