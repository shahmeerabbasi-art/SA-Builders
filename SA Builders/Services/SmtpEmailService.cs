using System.Net;
using System.Net.Mail;

namespace SA_Builders.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendActivationEmailAsync(string toEmail, string activationLink)
        {
            var subject = "Activate Your Account - SA Builders";
            var body = $@"
                <h2>Welcome to SA Builders</h2>
                <p>Please click the link below to activate your account and set your password:</p>
                <p><a href='{activationLink}'>{activationLink}</a></p>
                <p>If you did not request this, please ignore this email.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var subject = "Reset Your Password - SA Builders";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password. Click the link below to set a new password:</p>
                <p><a href='{resetLink}'>{resetLink}</a></p>
                <p>This link will expire in 1 hour. If you didn't request this, ignore this email.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var host = _config["Smtp:Host"];
                var port = int.Parse(_config["Smtp:Port"] ?? "587");
                var username = _config["Smtp:Username"];
                var password = _config["Smtp:Password"];
                var fromEmail = _config["Smtp:FromEmail"] ?? username;
                var fromName = _config["Smtp:FromName"] ?? "SA Builders";

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email successfully sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}