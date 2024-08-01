using Domain.Abstractions;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;

    public EmailService(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }

    public async Task SendEmailAsync(MailMessage mailMessage)
    {
        await _smtpClient.SendMailAsync(mailMessage);
    }
}
