namespace device_drop_backend.Dtos;

public class RegisterDto
{
    public string FirstName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class VerifyCodeDto
{
    public string Code { get; set; }
}