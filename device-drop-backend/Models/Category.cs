namespace device_drop_backend.Models;

public class Category : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    
    public ICollection<Product> Products { get; set; } = new List<Product>();
}