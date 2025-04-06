namespace device_drop_backend.Models;

public class Cart : BaseEntity
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public int TotalAmount { get; set; } = 0;

    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}