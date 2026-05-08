namespace Infrastructure.Services
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string From { get; set; } = "noreply@novahire.com";
        public string SenderEmail { get; set; } = "noreply@novahire.com";
        public string SenderName { get; set; } = "NovaHire";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;

        // URLs used in emails; set in configuration
        public string? ConfirmationUrl { get; set; }
        public string? ResetPasswordUrl { get; set; }
    }
}
