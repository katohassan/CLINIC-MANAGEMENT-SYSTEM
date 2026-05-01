using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace ClinicAppointmentSystem.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // For demonstration, we just log the email content.
        // In a real application, you would integrate SendGrid, SMTP, etc.
        _logger.LogInformation("Sending email to {Email}", email);
        _logger.LogInformation("Subject: {Subject}", subject);
        _logger.LogInformation("Message: {Message}", htmlMessage);

        return Task.CompletedTask;
    }
}
