namespace device_drop_backend.Models;

public class ProductVariant : BaseEntity
{
    public int Id { get; set; }
    public int ColorId { get; set; }
    public int Price { get; set; }
    public int? SalePrice { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Color Color { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}