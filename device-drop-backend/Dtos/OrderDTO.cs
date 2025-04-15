namespace device_drop_backend.Dtos;

public class OrderCartItemDTO
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public int ProductVariantId { get; set; }
    public ProductInfo Product { get; set; }
    public VariantInfo Variant { get; set; }
}

public class ProductInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Brand { get; set; }
}

public class VariantInfo
{
    public int Id { get; set; }
    public int Price { get; set; }
    public int? SalePrice { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; }
    public int? ColorId { get; set; }
}