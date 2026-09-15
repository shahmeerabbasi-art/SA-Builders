namespace SA_Builders.Services
{
    public interface IEmailService
    {
        Task SendActivationEmailAsync(string toEmail, string activationLink);
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }

    // Stub — swap for a real provider (SendGrid/SMTP) before production.
    public class ConsoleEmailService : IEmailService
    {
        private readonly ILogger<ConsoleEmailService> _logger;

        public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendActivationEmailAsync(string toEmail, string activationLink)
        {
            _logger.LogInformation("Activation email to {Email}: {Link}", toEmail, activationLink);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            _logger.LogInformation("Password reset email to {Email}: {Link}", toEmail, resetLink);
            return Task.CompletedTask;
        }
    }
}