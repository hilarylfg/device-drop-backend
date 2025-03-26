namespace device_drop_backend.Dtos;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Brand { get; set; }
    public CategoryDto Category { get; set; }
    public int CategoryId { get; set; }
    public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
}

public class ProductVariantDto
{
    public int Id { get; set; }
    public string Color { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; }
}