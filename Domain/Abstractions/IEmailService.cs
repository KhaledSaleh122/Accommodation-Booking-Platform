using System.Net.Mail;

namespace Domain.Abstractions
{
    public interface IEmailService
    {
        Task SendEmailAsync(MailMessage mailMessage);
    }
}
