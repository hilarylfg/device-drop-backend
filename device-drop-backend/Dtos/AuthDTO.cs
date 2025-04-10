namespace device_drop_backend.Dtos;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterDto
{
    public string FirstName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class OAuthDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
}

public class VerifyCodeDto
{
    public string Code { get; set; } = string.Empty;
}

public class VerifyTokenDto
{
    public string AuthToken { get; set; } = string.Empty;
}