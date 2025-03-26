namespace device_drop_backend.Models;

public class ProductVariant : BaseEntity
{
    public int Id { get; set; }
    public string Color { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}