namespace device_drop_backend.Models;

public class Color : BaseEntity
{
    public int Id { get; set; }
    public string Hex { get; set; } = string.Empty;
    public string NameRu { get; set; } = string.Empty;
    public string? NameEn { get; set; }

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}