namespace device_drop_backend.Dtos;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public List<ProductDto>? Products { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public List<ProductVariantDto>? Variants { get; set; }
}

public class ProductVariantDto
{
    public int Id { get; set; }
    public int ColorId { get; set; }
    public ColorDto? Color { get; set; }
    public int Price { get; set; }
    public int? SalePrice { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public ProductDto? Product { get; set; }
}

public class ColorDto
{
    public int Id { get; set; }
    public string Hex { get; set; } = string.Empty;
    public string NameRu { get; set; } = string.Empty;
    public string? NameEn { get; set; }
}

public class CheckoutFormValuesDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}