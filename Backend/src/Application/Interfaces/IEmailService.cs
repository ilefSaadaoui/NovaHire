using System.Threading.Tasks;

namespace Application.Interfaces
{
    /// <summary>
    /// Interface du service d'envoi d'emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envoyer un email de confirmation
        /// </summary>
        Task<bool> SendEmailConfirmationAsync(string email, string token);

        /// <summary>
        /// Envoyer un email de réinitialisation de mot de passe
        /// </summary>
        Task<bool> SendPasswordResetAsync(string email, string token);

        /// <summary>
        /// Envoyer un email de bienvenue
        /// </summary>
        Task<bool> SendWelcomeEmailAsync(string email, string firstName);

        /// <summary>
        /// Envoyer une notification de candidature
        /// </summary>
        Task<bool> SendApplicationNotificationAsync(string recruiterEmail, string jobTitle, string candidateName);

        /// <summary>
        /// Envoyer une notification de statut de candidature
        /// </summary>
        Task<bool> SendApplicationStatusUpdateAsync(string candidateEmail, string jobTitle, string status);

        /// <summary>
        /// Envoyer une invitation de recruteur avec ses identifiants temporaires
        /// </summary>
        Task<bool> SendRecruiterInvitationAsync(string recruiterEmail, string tempPassword, string companyName, string adminName);

        /// <summary>
        /// Envoyer un email de confirmation de candidature au candidat
        /// </summary>
        Task<bool> SendApplicationConfirmationAsync(string candidateEmail, string candidateName, string jobTitle, string companyName);

        /// <summary>
        /// Envoyer un email générique
        /// </summary>
        Task<bool> SendEmailAsync(string email, string subject, string body);

        /// <summary>
        /// Envoyer une invitation d'entretien au candidat, avec Reply-To configuré sur l'e-mail du recruteur
        /// </summary>
        Task<bool> SendInterviewInvitationAsync(string candidateEmail, string candidateName, string subject, string htmlBody, string recruiterEmail, string recruiterName);
        
        /// <summary>
        /// Envoyer un email d'activation de compte après approbation SuperAdmin
        /// </summary>
        Task<bool> SendAccountActivationAsync(string email, string firstName, string companyName);

        /// <summary>
        /// Envoyer une invitation de quiz IA au candidat
        /// </summary>
        Task<bool> SendQuizInvitationAsync(string candidateEmail, string candidateName, string jobTitle, string quizUrl, string companyName);

        /// <summary>
        /// Envoyer un email de refus personnalisé au candidat
        /// </summary>
        Task<bool> SendRejectionEmailAsync(string candidateEmail, string candidateName, string jobTitle, string companyName, string? reason = null);

        /// <summary>
        /// Envoyer une notification de mise à l'étude (shortlisted) au candidat
        /// </summary>
        Task<bool> SendShortlistedEmailAsync(string candidateEmail, string candidateName, string jobTitle, string companyName);
        Task<bool> SendInterviewInvitationWithCalendarAsync(string candidateEmail, string candidateName, string subject, string htmlBody, DateTime scheduledAt, int durationMinutes, string location);
    }
}
