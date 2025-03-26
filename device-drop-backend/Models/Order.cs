namespace device_drop_backend.Models;

public class Order : BaseEntity
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Token { get; set; }
    public int TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string? PaymentId { get; set; }
    public string Items { get; set; } 
    public string FirstName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string? Comment { get; set; }

    public User? User { get; set; }
}

public enum OrderStatus
{
    PENDING,
    SUCCEEDED,
    CANCELLED
}