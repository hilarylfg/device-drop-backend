namespace device_drop_backend.Models;

public class User : BaseEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.USER;
    public DateTime? Verified { get; set; }
    public string? Provider { get; set; }
    public string? ProviderId { get; set; }

    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public VerificationCode? VerificationCode { get; set; }
}

public enum UserRole
{
    USER,
    ADMIN
}