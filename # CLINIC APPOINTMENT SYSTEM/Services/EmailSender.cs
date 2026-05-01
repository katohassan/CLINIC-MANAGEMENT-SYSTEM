using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClinicAppointmentSystem.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IConfiguration _configuration;

    public EmailSender(ILogger<EmailSender> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("Attempting to send email to {Email}", email);

        var mailHost = _configuration["EmailSettings:Host"] ?? "smtp.gmail.com";
        var mailPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
        var mailUser = _configuration["EmailSettings:Username"] ?? "ainembabazidianah101@gmail.com";
        var mailPass = _configuration["EmailSettings:Password"]; // You must configure this via secrets or appsettings.json!

        if (string.IsNullOrEmpty(mailPass))
        {
            _logger.LogWarning("Email sending bypassed because no password is provided in configuration.");
            return;
        }

        try
        {
            var client = new SmtpClient(mailHost, mailPort)
            {
                Credentials = new NetworkCredential(mailUser, mailPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(mailUser, "Clinic Appointment System"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email successfully sent to {Email}", email);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
        }
    }
}
