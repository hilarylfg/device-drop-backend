namespace device_drop_backend.Models;

public class VerificationCode : BaseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(2);

    public User User { get; set; } = null!;
}