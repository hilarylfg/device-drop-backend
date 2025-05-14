namespace device_drop_backend.Models;

public class Order : BaseEntity
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string? PaymentId { get; set; }
    public string Items { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Comment { get; set; }

    public User? User { get; set; }
}

public enum OrderStatus
{
    PENDING,
    SUCCEEDED,
    CANCELLED
}