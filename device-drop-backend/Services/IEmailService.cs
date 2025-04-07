namespace device_drop_backend.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}