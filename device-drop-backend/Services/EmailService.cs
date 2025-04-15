using System.Net.Mail;

namespace device_drop_backend.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        var smtpHost = _configuration["Smtp:Host"] ??
                       throw new InvalidOperationException("Smtp:Host is not configured");
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ??
                                 throw new InvalidOperationException("Smtp:Port is not configured"));
        var smtpUsername = _configuration["Smtp:Username"] ??
                           throw new InvalidOperationException("Smtp:Username is not configured");
        var smtpPassword = _configuration["Smtp:Password"] ??
                           throw new InvalidOperationException("Smtp:Password is not configured");
        var fromAddress = _configuration["Smtp:From"] ?? "device-drop@test-y7zpl98ez8r45vx6.mlsender.net";

        using var smtpClient = new SmtpClient
        {
            Host = smtpHost,
            Port = smtpPort,
            Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromAddress),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };
        mailMessage.To.Add(to);

        await smtpClient.SendMailAsync(mailMessage);
    }
}