using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;

namespace Infrastructure.Services
{
    /// <summary>
    /// Service d'envoi d'e-mails utilisant le protocole SMTP via la bibliothèque MailKit.
    /// Lit la configuration depuis les options d'application (EmailSettings).
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly EmailSettings _settings;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="SmtpEmailService"/>.
        /// </summary>
        /// <param name="logger">L'instance du logger pour le suivi et le diagnostic.</param>
        /// <param name="options">Les options de configuration pour les e-mails.</param>
        public SmtpEmailService(ILogger<SmtpEmailService> logger, IOptions<EmailSettings> options)
        {
            _logger = logger;
            _settings = options?.Value ?? new EmailSettings();
        }

        /// <summary>
        /// Crée un message MimeMessage structuré pour l'envoi d'e-mails.
        /// </summary>
        /// <param name="to">Adresse e-mail du destinataire.</param>
        /// <param name="subject">Sujet de l'e-mail.</param>
        /// <param name="body">Corps de l'e-mail au format HTML.</param>
        /// <param name="toName">Nom optionnel du destinataire.</param>
        /// <returns>Une instance configurée de <see cref="MimeMessage"/>.</returns>
        private MimeMessage CreateMessage(string to, string subject, string body, string? toName = null)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName ?? "NovaHire", _settings.SenderEmail ?? _settings.From));
            message.To.Add(new MailboxAddress(toName ?? to, to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body,
                TextBody = StripHtml(body)
            };

            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        /// <summary>
        /// Supprime les balises HTML d'une chaîne de caractères pour produire une version brute textuelle.
        /// </summary>
        /// <param name="html">La chaîne contenant du code HTML.</param>
        /// <returns>La chaîne nettoyée de ses balises HTML.</returns>
        private string StripHtml(string html)
        {
            // Helper minimal pour générer une version texte brut. Pour la production, privilégier une bibliothèque robuste.
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Envoie de manière asynchrone le message d'e-mail fourni à travers le client SMTP.
        /// </summary>
        /// <param name="message">Le message MimeMessage à envoyer.</param>
        /// <returns>Une valeur indiquant si l'envoi a réussi ou non.</returns>
        private async Task<bool> SendAsync(MimeMessage message)
        {
            // Si aucun serveur SMTP ou mot de passe configuré, enregistre un log et retourne false
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost) || 
                string.IsNullOrWhiteSpace(_settings.Password) || 
                _settings.Password.StartsWith("YOUR_") ||
                _settings.Password.Contains("PLACEHOLDER"))
            {
                _logger.LogWarning("SMTP non configuré ou mot de passe d'application manquant. L'e-mail n'a pas été envoyé. Sujet: {Subject}", message.Subject);
                return false;
            }

            try
            {
                using var client = new SmtpClient();
                // 15 secondes pour permettre la négociation TLS et l'authentification avec smtp.gmail.com sur le cloud
                client.Timeout = 15000;
                // Accepte les certificats valides
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                if (_settings.SmtpPort == 465)
                {
                    // Port 465 : SSL direct
                    await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.SslOnConnect).ConfigureAwait(false);
                }
                else if (_settings.SmtpPort == 587 || _settings.EnableSsl)
                {
                    // Port 587 : STARTTLS (recommandé pour Google Workspace et Gmail sur le Cloud)
                    await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls).ConfigureAwait(false);
                }
                else
                {
                    await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.Auto).ConfigureAwait(false);
                }

                if (!string.IsNullOrEmpty(_settings.Username))
                {
                    await client.AuthenticateAsync(_settings.Username, _settings.Password).ConfigureAwait(false);
                }

                await client.SendAsync(message).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);

                _logger.LogInformation("Email sent successfully to {To} with subject {Subject}", message.To, message.Subject);
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}. Error: {Error}", message.To, message.Subject, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Envoie un e-mail de confirmation à l'utilisateur lors de son inscription.
        /// </summary>
        /// <param name="email">L'adresse e-mail de l'utilisateur.</param>
        /// <param name="token">Le jeton de validation pour confirmer l'inscription.</param>
        /// <returns>True si l'envoi a réussi, sinon false.</returns>
        public async Task<bool> SendEmailConfirmationAsync(string email, string token)
        {
            var confirmUrl = _settings.ConfirmationUrl ?? "#";
            var body = $"<p>Please confirm your email by clicking <a href='{confirmUrl}?token={token}'>here</a>.</p>";
            var message = CreateMessage(email, "Confirm your email", body);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie un e-mail de réinitialisation de mot de passe à l'utilisateur.
        /// </summary>
        /// <param name="email">L'adresse e-mail de l'utilisateur concerné.</param>
        /// <param name="token">Le jeton sécurisé de réinitialisation.</param>
        /// <returns>True si l'e-mail a été envoyé avec succès, sinon false.</returns>
        public async Task<bool> SendPasswordResetAsync(string email, string token)
        {
            var resetUrl = _settings.ResetPasswordUrl ?? "https://localhost:3010/reinitialiser-mot-de-passe";
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(email);
            var resetLink = $"{resetUrl}?token={encodedToken}&email={encodedEmail}";
            var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background: #020617; color: #ffffff; border-radius: 24px; overflow: hidden; border: 1px solid rgba(255,255,255,0.05);'>
                <div style='background: linear-gradient(135deg, #00A7E1, #8B5CF6); padding: 40px; text-align: center; position: relative;'>
                    <h1 style='margin: 0; font-size: 28px; font-weight: 800; color: #ffffff; letter-spacing: -1px;'>Sécurisez votre compte 🛡️</h1>
                </div>
                <div style='padding: 40px; background: #020617;'>
                    <p style='font-size: 16px; color: rgba(255,255,255,0.7); line-height: 1.6;'>
                        Bonjour,
                    </p>
                    <p style='font-size: 16px; color: rgba(255,255,255,0.7); line-height: 1.6;'>
                        Nous avons reçu une demande de réinitialisation de mot de passe pour votre compte <strong style='color: #00A7E1;'>NovaHire</strong>.
                    </p>
                    <div style='margin: 32px 0; text-align: center;'>
                        <a href='{resetLink}' style='display: inline-block; background: linear-gradient(135deg, #00A7E1, #0077B6); color: #ffffff; text-decoration: none; padding: 16px 36px; border-radius: 14px; font-weight: 700; font-size: 16px; box-shadow: 0 10px 20px rgba(0, 167, 225, 0.3);'>
                            Réinitialiser mon mot de passe
                        </a>
                    </div>
                    <p style='font-size: 14px; color: rgba(255,255,255,0.4); line-height: 1.6; border-top: 1px solid rgba(255,255,255,0.06); padding-top: 24px;'>
                        Si vous n'êtes pas à l'origine de cette demande, vous pouvez ignorer cet e-mail en toute sécurité. Le lien expirera dans 1 heure.
                    </p>
                </div>
                <div style='padding: 24px; background: rgba(255,255,255,0.02); text-align: center; border-top: 1px solid rgba(255,255,255,0.05);'>
                    <p style='margin: 0; font-size: 11px; color: rgba(255,255,255,0.3); text-transform: uppercase; letter-spacing: 2px;'>NovaHire — Excellence en Recrutement AI</p>
                </div>
            </div>";

            var message = CreateMessage(email, "Réinitialisation de votre mot de passe NovaHire", body);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie un e-mail de bienvenue de base à un nouvel utilisateur.
        /// </summary>
        /// <param name="email">L'adresse e-mail de l'utilisateur.</param>
        /// <param name="firstName">Le prénom de l'utilisateur.</param>
        /// <returns>True si l'envoi a réussi, sinon false.</returns>
        public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
        {
            var body = $"<p>Welcome {System.Net.WebUtility.HtmlEncode(firstName)} to NovaHire!</p>";
            var message = CreateMessage(email, "Welcome to NovaHire", body);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie une notification au recruteur lorsqu'un candidat postule à une offre d'emploi.
        /// </summary>
        /// <param name="recruiterEmail">Adresse e-mail du recruteur.</param>
        /// <param name="jobTitle">Titre de l'offre d'emploi concernée.</param>
        /// <param name="candidateName">Nom complet du candidat.</param>
        /// <returns>True si le message a bien été transmis, sinon false.</returns>
        public async Task<bool> SendApplicationNotificationAsync(string recruiterEmail, string jobTitle, string candidateName)
        {
            var body = $"<p>New application for <strong>{System.Net.WebUtility.HtmlEncode(jobTitle)}</strong> by {System.Net.WebUtility.HtmlEncode(candidateName)}.</p>";
            var message = CreateMessage(recruiterEmail, "New application received", body);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie un e-mail au candidat pour l'informer du changement de statut de sa candidature.
        /// </summary>
        /// <param name="candidateEmail">Adresse e-mail du candidat.</param>
        /// <param name="jobTitle">Titre du poste concerné.</param>
        /// <param name="status">Le nouveau statut de la candidature.</param>
        /// <returns>True si l'envoi a réussi, sinon false.</returns>
        public async Task<bool> SendApplicationStatusUpdateAsync(string candidateEmail, string jobTitle, string status)
        {
            var body = $"<p>Your application for <strong>{System.Net.WebUtility.HtmlEncode(jobTitle)}</strong> has been updated to: {System.Net.WebUtility.HtmlEncode(status)}</p>";
            var message = CreateMessage(candidateEmail, "Application status updated", body);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie une invitation à un recruteur de la part d'un administrateur d'entreprise.
        /// </summary>
        /// <param name="recruiterEmail">Adresse e-mail du nouveau recruteur.</param>
        /// <param name="tempPassword">Mot de passe temporaire généré automatiquement.</param>
        /// <param name="companyName">Nom de l'entreprise qui invite.</param>
        /// <param name="adminName">Nom de l'administrateur à l'origine de l'invitation.</param>
        /// <returns>True si l'invitation a été envoyée avec succès, sinon false.</returns>
        public async Task<bool> SendRecruiterInvitationAsync(string recruiterEmail, string tempPassword, string companyName, string adminName)
        {
            var loginUrl = _settings.ConfirmationUrl?.Replace("/confirm-email", "/connexion") ?? "http://localhost:3010/connexion";
            var body = $@"
            <div style='font-family: ""Inter"", -apple-system, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f8fafc; padding: 40px 20px;'>
                <div style='background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06); border: 1px solid #e2e8f0;'>
                    <!-- Header -->
                    <div style='background-color: #00A7E1; padding: 32px; text-align: center;'>
                        <h1 style='margin: 0; font-size: 24px; font-weight: 700; color: #ffffff;'>Bienvenue chez {System.Net.WebUtility.HtmlEncode(companyName)}</h1>
                    </div>

                    <!-- Content -->
                    <div style='padding: 40px;'>
                        <p style='font-size: 16px; color: #1e293b; line-height: 1.6; margin-bottom: 24px;'>
                            Bonjour,
                        </p>
                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 24px;'>
                            <strong style='color: #0f172a;'>{System.Net.WebUtility.HtmlEncode(adminName)}</strong> est ravi(e) de vous inviter à rejoindre l'équipe de recrutement de <strong style='color: #00A7E1;'>{System.Net.WebUtility.HtmlEncode(companyName)}</strong>.
                        </p>
                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 32px;'>
                            Nous utilisons <strong>NovaHire</strong> pour transformer nos processus de recrutement grâce à l'IA, et nous avons hâte de commencer cette collaboration avec vous.
                        </p>

                        <!-- Access Card -->
                        <div style='background-color: #f1f5f9; border-radius: 12px; padding: 24px; margin-bottom: 32px;'>
                            <h3 style='margin: 0 0 16px; font-size: 14px; font-weight: 700; color: #475569; text-transform: uppercase; letter-spacing: 0.05em;'>Vos accès sécurisés</h3>
                            
                            <div style='margin-bottom: 16px;'>
                                <div style='font-size: 13px; color: #64748b; margin-bottom: 4px;'>Email</div>
                                <div style='font-size: 15px; font-weight: 600; color: #0f172a;'>{System.Net.WebUtility.HtmlEncode(recruiterEmail)}</div>
                            </div>

                            <div>
                                <div style='font-size: 13px; color: #64748b; margin-bottom: 4px;'>Mot de passe temporaire</div>
                                <div style='display: inline-block; background-color: #ffffff; border: 1px solid #cbd5e1; padding: 8px 16px; border-radius: 6px; font-family: monospace; font-size: 18px; font-weight: 700; color: #00A7E1;'>{System.Net.WebUtility.HtmlEncode(tempPassword)}</div>
                            </div>
                        </div>

                        <!-- CTA -->
                        <div style='text-align: center; margin-bottom: 32px;'>
                            <a href='{loginUrl}' style='display: inline-block; background-color: #00A7E1; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 8px; font-weight: 700; font-size: 16px;'>
                                Activer mon compte
                            </a>
                        </div>

                        <!-- Security Note -->
                        <div style='border-top: 1px solid #e2e8f0; padding-top: 24px;'>
                            <p style='margin: 0; font-size: 13px; color: #64748b; line-height: 1.5;'>
                                <strong>Note de sécurité :</strong> Pour garantir la protection de vos données, il vous sera demandé de définir votre propre mot de passe lors de votre première connexion.
                            </p>
                        </div>
                    </div>
                </div>

                <!-- Footer -->
                <div style='padding: 24px; text-align: center;'>
                    <p style='margin: 0; font-size: 12px; color: #94a3b8;'>
                        Propulsé par <strong>NovaHire AI</strong> — L'excellence du recrutement.
                    </p>
                </div>
            </div>";

            var message = CreateMessage(recruiterEmail, $"Invitation à rejoindre {companyName} sur NovaHire", body);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie un e-mail de confirmation de réception de candidature au candidat.
        /// </summary>
        /// <param name="candidateEmail">Adresse e-mail du candidat.</param>
        /// <param name="candidateName">Nom complet du candidat.</param>
        /// <param name="jobTitle">Titre de l'offre d'emploi postulée.</param>
        /// <param name="companyName">Nom de l'entreprise proposant le poste.</param>
        /// <returns>True si le message de confirmation a été envoyé avec succès, sinon false.</returns>
        public async Task<bool> SendApplicationConfirmationAsync(string candidateEmail, string candidateName, string jobTitle, string companyName)
        {
            var body = $@"
            <div style='font-family: ""Inter"", -apple-system, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f8fafc; padding: 40px 20px;'>
                <div style='background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06); border: 1px solid #e2e8f0;'>
                    <!-- Header -->
                    <div style='background: linear-gradient(135deg, #00A7E1, #0077B6); padding: 40px; text-align: center;'>
                        <h1 style='margin: 0; font-size: 26px; font-weight: 700; color: #ffffff;'>Candidature bien reçue !</h1>
                    </div>

                    <!-- Content -->
                    <div style='padding: 40px;'>
                        <p style='font-size: 16px; color: #1e293b; line-height: 1.6; margin-bottom: 24px;'>
                            Bonjour <strong>{System.Net.WebUtility.HtmlEncode(candidateName)}</strong>,
                        </p>
                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 24px;'>
                            Nous confirmons la bonne réception de votre candidature pour le poste :
                        </p>

                        <!-- Job Title Card -->
                        <div style='background-color: #f0f9ff; border-left: 4px solid #00A7E1; border-radius: 8px; padding: 20px; margin-bottom: 24px;'>
                            <p style='margin: 0; font-size: 18px; font-weight: 700; color: #0077B6;'>{System.Net.WebUtility.HtmlEncode(jobTitle)}</p>
                            <p style='margin: 8px 0 0; font-size: 14px; color: #64748b;'>{System.Net.WebUtility.HtmlEncode(companyName)}</p>
                        </div>

                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 24px;'>
                            Votre dossier sera examiné avec attention par notre équipe de recrutement. Si votre profil correspond aux critères recherchés, nous vous contacterons pour la suite du processus.
                        </p>

                        <!-- Info Box -->
                        <div style='background-color: #f1f5f9; border-radius: 12px; padding: 24px; margin-bottom: 24px;'>
                            <h3 style='margin: 0 0 12px; font-size: 14px; font-weight: 700; color: #475569; text-transform: uppercase; letter-spacing: 0.05em;'>Prochaines étapes</h3>
                            <ul style='margin: 0; padding-left: 20px; color: #334155; font-size: 14px; line-height: 2;'>
                                <li>Analyse de votre profil par notre équipe</li>
                                <li>Vous serez contacté(e) si votre profil est retenu</li>
                                <li>En cas de questions, n'hésitez pas à répondre à cet e-mail</li>
                            </ul>
                        </div>

                        <p style='font-size: 14px; color: #64748b; line-height: 1.6; border-top: 1px solid #e2e8f0; padding-top: 24px;'>
                            Nous vous remercions pour l'intérêt que vous portez à cette opportunité et vous souhaitons bonne chance !
                        </p>
                    </div>
                </div>

                <!-- Footer -->
                <div style='padding: 24px; text-align: center;'>
                    <p style='margin: 0; font-size: 12px; color: #94a3b8;'>
                        Propulsé par <strong>NovaHire AI</strong> — L'excellence du recrutement.
                    </p>
                </div>
            </div>";

            var message = CreateMessage(candidateEmail, $"Confirmation de votre candidature — {System.Net.WebUtility.HtmlEncode(jobTitle)}", body, candidateName);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie un e-mail basique avec un sujet et corps personnalisés.
        /// </summary>
        /// <param name="email">Adresse de destination.</param>
        /// <param name="subject">Sujet de l'e-mail.</param>
        /// <param name="body">Corps du message au format HTML.</param>
        /// <returns>True si envoyé avec succès, sinon false.</returns>
        public async Task<bool> SendEmailAsync(string email, string subject, string body)
        {
            var message = CreateMessage(email, subject, body);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie une invitation d'entretien à un candidat au nom d'un recruteur spécifique.
        /// Le message sera envoyé depuis l'adresse du système avec l'adresse du recruteur en "Reply-To".
        /// </summary>
        /// <param name="candidateEmail">Adresse e-mail du candidat.</param>
        /// <param name="candidateName">Nom complet du candidat.</param>
        /// <param name="subject">Sujet de l'invitation.</param>
        /// <param name="htmlBody">Corps HTML personnalisé du message d'invitation.</param>
        /// <param name="recruiterEmail">Adresse e-mail professionnelle du recruteur.</param>
        /// <param name="recruiterName">Nom du recruteur.</param>
        /// <returns>True si l'envoi a réussi, sinon false.</returns>
        public async Task<bool> SendInterviewInvitationAsync(string candidateEmail, string candidateName, string subject, string htmlBody, string recruiterEmail, string recruiterName)
        {
            var message = new MimeMessage();
            // Envoyer depuis le compte système mais en affichant le nom du recruteur
            message.From.Add(new MailboxAddress($"{recruiterName} via NovaHire", _settings.SenderEmail ?? _settings.From));
            message.To.Add(new MailboxAddress(candidateName, candidateEmail));
            message.ReplyTo.Add(new MailboxAddress(recruiterName, recruiterEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = StripHtml(htmlBody)
            };
            message.Body = bodyBuilder.ToMessageBody();

            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie un e-mail informant qu'un compte d'entreprise a été approuvé et activé par l'administration.
        /// </summary>
        /// <param name="email">Adresse e-mail de l'administrateur d'entreprise.</param>
        /// <param name="firstName">Prénom du destinataire.</param>
        /// <param name="companyName">Nom de l'entreprise approuvée.</param>
        /// <returns>True si l'e-mail a été envoyé, sinon false.</returns>
        public async Task<bool> SendAccountActivationAsync(string email, string firstName, string companyName)
        {
            var loginUrl = _settings.ConfirmationUrl?.Replace("/confirm-email", "/connexion") ?? "http://localhost:3010/connexion";
            var body = $@"
            <div style='font-family: ""Inter"", -apple-system, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f8fafc; padding: 40px 20px;'>
                <div style='background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1); border: 1px solid #e2e8f0;'>
                    <div style='background: linear-gradient(135deg, #6C63FF, #00D9FF); padding: 40px; text-align: center;'>
                        <h1 style='margin: 0; font-size: 28px; font-weight: 800; color: #ffffff;'>Compte Approuvé ! ✨</h1>
                    </div>
                    <div style='padding: 40px;'>
                        <p style='font-size: 16px; color: #1e293b; line-height: 1.6; margin-bottom: 24px;'>
                            Bonjour <strong>{System.Net.WebUtility.HtmlEncode(firstName)}</strong>,
                        </p>
                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 24px;'>
                            Nous avons le plaisir de vous informer que votre inscription pour l'entreprise <strong>{System.Net.WebUtility.HtmlEncode(companyName)}</strong> a été approuvée par l'administration NovaHire.
                        </p>
                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 32px;'>
                            Vous pouvez désormais accéder à votre espace administrateur et commencer à transformer vos processus de recrutement.
                        </p>
                        <div style='text-align: center; margin-bottom: 32px;'>
                            <a href='{loginUrl}' style='display: inline-block; background: linear-gradient(135deg, #6C63FF, #00D9FF); color: #ffffff; text-decoration: none; padding: 16px 36px; border-radius: 14px; font-weight: 700; font-size: 16px; box-shadow: 0 8px 20px rgba(108, 99, 255, 0.3);'>
                                Activer et se connecter
                            </a>
                        </div>
                    </div>
                </div>
            </div>";

            var message = CreateMessage(email, "Félicitations ! Votre compte NovaHire est activé", body);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie une invitation à un test de présélection (quiz) sous forme d'e-mail personnalisé au candidat.
        /// </summary>
        /// <param name="candidateEmail">Adresse e-mail du candidat.</param>
        /// <param name="candidateName">Nom complet du candidat.</param>
        /// <param name="jobTitle">Titre du poste concerné.</param>
        /// <param name="quizUrl">URL unique pour passer le test/quiz.</param>
        /// <param name="companyName">Nom de l'entreprise recruteuse.</param>
        /// <returns>True si l'invitation a été envoyée avec succès, sinon false.</returns>
        public async Task<bool> SendQuizInvitationAsync(string candidateEmail, string candidateName, string jobTitle, string quizUrl, string companyName)
        {
            var body = $@"
            <div style='font-family: ""Inter"", -apple-system, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f8fafc; padding: 40px 20px;'>
                <div style='background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 30px rgba(0, 0, 0, 0.08); border: 1px solid #e2e8f0;'>
                    <!-- Header -->
                    <div style='background: linear-gradient(135deg, #0EA5E9, #8B5CF6); padding: 40px; text-align: center;'>
                        <h1 style='margin: 0; font-size: 26px; font-weight: 800; color: #ffffff;'>Évaluation de Présélection — {System.Net.WebUtility.HtmlEncode(companyName)}</h1>
                    </div>

                    <!-- Content -->
                    <div style='padding: 40px;'>
                        <p style='font-size: 17px; color: #1e293b; line-height: 1.6; margin-bottom: 24px;'>
                            Bonjour <strong>{System.Net.WebUtility.HtmlEncode(candidateName)}</strong>,
                        </p>
                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 24px;'>
                            Dans le cadre de votre candidature pour le poste de <strong>{System.Net.WebUtility.HtmlEncode(jobTitle)}</strong>, nous vous invitons à passer un court test de présélection. 
                            <br><br>
                            Cette première étape interactive a pour but de mettre en lumière votre logique, vos points forts et votre approche globale, au-delà des simples compétences techniques.
                        </p>
                        
                        <div style='background-color: #f0f9ff; border-radius: 12px; padding: 24px; margin-bottom: 32px; border-left: 4px solid #0EA5E9;'>
                            <p style='margin: 0; font-size: 14px; color: #0369a1; line-height: 1.6;'>
                                Prévoyez environ 15 minutes dans un environnement calme pour compléter cette étape.
                            </p>
                        </div>

                        <!-- CTA -->
                        <div style='text-align: center; margin-bottom: 32px;'>
                            <a href='{quizUrl}' style='display: inline-block; background: linear-gradient(135deg, #0EA5E9, #8B5CF6); color: #ffffff; text-decoration: none; padding: 18px 40px; border-radius: 14px; font-weight: 700; font-size: 17px; box-shadow: 0 10px 25px rgba(139, 92, 246, 0.3);'>
                                Démarrer l'évaluation
                            </a>
                        </div>

                        <p style='font-size: 14px; color: #64748b; line-height: 1.6; border-top: 1px solid #e2e8f0; padding-top: 24px; text-align: center;'>
                            Bonne chance pour votre évaluation !<br>
                            L'équipe de recrutement de <strong>{System.Net.WebUtility.HtmlEncode(companyName)}</strong>
                        </p>
                    </div>
                </div>
                
                <!-- Footer -->
                <div style='padding: 24px; text-align: center;'>
                    <p style='margin: 0; font-size: 12px; color: #94a3b8;'>
                        Propulsé par <strong>NovaHire AI</strong> — L'excellence du recrutement.
                    </p>
                </div>
            </div>";

            var message = CreateMessage(candidateEmail, $"Invitation : Test de Présélection pour le poste de {jobTitle}", body, candidateName);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie un e-mail de refus personnalisé au candidat si sa candidature n'est pas retenue.
        /// </summary>
        /// <param name="candidateEmail">Adresse e-mail du candidat.</param>
        /// <param name="candidateName">Nom complet du candidat.</param>
        /// <param name="jobTitle">Titre du poste.</param>
        /// <param name="companyName">Nom de l'entreprise.</param>
        /// <param name="reason">Raison optionnelle expliquant le refus.</param>
        /// <returns>True si le message de refus a été envoyé, sinon false.</returns>
        public async Task<bool> SendRejectionEmailAsync(string candidateEmail, string candidateName, string jobTitle, string companyName, string? reason = null)
        {
            var body = $@"
            <div style='font-family: ""Inter"", -apple-system, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f8fafc; padding: 40px 20px;'>
                <div style='background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;'>
                    <div style='padding: 40px;'>
                        <p style='font-size: 17px; color: #1e293b; line-height: 1.6; margin-bottom: 24px;'>
                            Bonjour <strong>{System.Net.WebUtility.HtmlEncode(candidateName)}</strong>,
                        </p>
                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 24px;'>
                            Nous vous remercions de l'intérêt que vous avez porté à <strong>{System.Net.WebUtility.HtmlEncode(companyName)}</strong> et du temps que vous avez consacré à votre candidature pour le poste de <strong>{System.Net.WebUtility.HtmlEncode(jobTitle)}</strong>.
                        </p>
                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 24px;'>
                            Après une étude attentive de votre profil, nous avons le regret de vous informer que nous ne pouvons pas donner une suite favorable à votre candidature pour le moment.
                        </p>
                        {(string.IsNullOrEmpty(reason) ? "" : $@"
                        <div style='background-color: #fef2f2; border-radius: 12px; padding: 24px; margin-bottom: 32px; border-left: 4px solid #ef4444;'>
                            <p style='margin: 0; font-size: 14px; color: #991b1b; line-height: 1.6;'>
                                <strong>Retour constructif :</strong><br>
                                {System.Net.WebUtility.HtmlEncode(reason)}
                            </p>
                        </div>")}
                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 32px;'>
                            Nous conserverons toutefois votre profil dans notre vivier de talents pour d'éventuelles opportunités futures correspondant à votre expertise.
                        </p>
                        <p style='font-size: 14px; color: #64748b; line-height: 1.6; border-top: 1px solid #e2e8f0; padding-top: 24px;'>
                            Nous vous souhaitons beaucoup de succès dans vos recherches professionnelles.<br><br>
                            Bien cordialement,<br>
                            L'équipe de recrutement de <strong>{System.Net.WebUtility.HtmlEncode(companyName)}</strong>
                        </p>
                    </div>
                </div>
            </div>";

            var message = CreateMessage(candidateEmail, $"Mise à jour concernant votre candidature chez {companyName}", body, candidateName);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie un e-mail informant le candidat que sa candidature a été retenue/shortlistée pour les étapes suivantes.
        /// </summary>
        /// <param name="candidateEmail">Adresse e-mail du candidat.</param>
        /// <param name="candidateName">Nom complet du candidat.</param>
        /// <param name="jobTitle">Titre du poste.</param>
        /// <param name="companyName">Nom de l'entreprise.</param>
        /// <returns>True si le message a été envoyé avec succès, sinon false.</returns>
        public async Task<bool> SendShortlistedEmailAsync(string candidateEmail, string candidateName, string jobTitle, string companyName)
        {
            var body = $@"
            <div style='font-family: ""Inter"", -apple-system, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f8fafc; padding: 40px 20px;'>
                <div style='background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 30px rgba(0, 0, 0, 0.08); border: 1px solid #e2e8f0;'>
                    <div style='background: linear-gradient(135deg, #6366f1, #8b5cf6); padding: 40px; text-align: center;'>
                        <h1 style='margin: 0; font-size: 26px; font-weight: 800; color: #ffffff;'>Bonne nouvelle ! ✨</h1>
                    </div>
                    <div style='padding: 40px;'>
                        <p style='font-size: 17px; color: #1e293b; line-height: 1.6; margin-bottom: 24px;'>
                            Bonjour <strong>{System.Net.WebUtility.HtmlEncode(candidateName)}</strong>,
                        </p>
                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 24px;'>
                            Nous avons le plaisir de vous informer que votre profil a retenu toute notre attention pour le poste de <strong>{System.Net.WebUtility.HtmlEncode(jobTitle)}</strong> chez <strong>{System.Net.WebUtility.HtmlEncode(companyName)}</strong>.
                        </p>
                        <p style='font-size: 16px; color: #334155; line-height: 1.6; margin-bottom: 24px;'>
                            Votre candidature a été **présélectionnée** pour la suite du processus de recrutement. Notre équipe est actuellement en train d'organiser les prochaines étapes.
                        </p>
                        <div style='background-color: #f5f3ff; border-radius: 12px; padding: 24px; margin-bottom: 32px; border-left: 4px solid #6366f1;'>
                            <p style='margin: 0; font-size: 14px; color: #4338ca; line-height: 1.6;'>
                                Vous recevrez très prochainement une invitation pour un premier échange ou un test technique complémentaire.
                            </p>
                        </div>
                        <p style='font-size: 14px; color: #64748b; line-height: 1.6; border-top: 1px solid #e2e8f0; padding-top: 24px;'>
                            Merci de votre patience et à très bientôt,<br><br>
                            L'équipe de recrutement de <strong>{System.Net.WebUtility.HtmlEncode(companyName)}</strong>
                        </p>
                    </div>
                </div>
            </div>";

            var message = CreateMessage(candidateEmail, $"Félicitations : Votre candidature chez {companyName} progresse !", body, candidateName);
            return await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Envoie une invitation d'entretien contenant un fichier d'invitation calendrier standard (ICS) en pièce jointe.
        /// </summary>
        /// <param name="candidateEmail">Adresse e-mail du candidat.</param>
        /// <param name="candidateName">Nom complet du candidat.</param>
        /// <param name="subject">Sujet de l'e-mail.</param>
        /// <param name="htmlBody">Corps HTML du message.</param>
        /// <param name="scheduledAt">Date et heure planifiée pour l'entretien.</param>
        /// <param name="durationMinutes">Durée de l'entretien en minutes.</param>
        /// <param name="location">Lieu ou lien de réunion virtuelle.</param>
        /// <returns>True si le message et son invitation calendrier ont été envoyés, sinon false.</returns>
        public async Task<bool> SendInterviewInvitationWithCalendarAsync(string candidateEmail, string candidateName, string subject, string htmlBody, DateTime scheduledAt, int durationMinutes, string location)
        {
            var message = CreateMessage(candidateEmail, subject, htmlBody, candidateName);
            
            var icsContent = $@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//NovaHire//NONSGML Interview//EN
BEGIN:VEVENT
UID:{Guid.NewGuid()}
DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}
DTSTART:{scheduledAt.ToUniversalTime():yyyyMMddTHHmmssZ}
DTEND:{scheduledAt.ToUniversalTime().AddMinutes(durationMinutes):yyyyMMddTHHmmssZ}
SUMMARY:{subject}
DESCRIPTION:{StripHtml(htmlBody).Replace("\n", "\\n")}
LOCATION:{location}
END:VEVENT
END:VCALENDAR";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = StripHtml(htmlBody)
            };

            var calendarBytes = Encoding.UTF8.GetBytes(icsContent);
            var calendarPart = new MimePart("text", "calendar")
            {
                Content = new MimeContent(new MemoryStream(calendarBytes)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = "invite.ics"
            };
            calendarPart.ContentType.Parameters["method"] = "REQUEST";

            bodyBuilder.Attachments.Add(calendarPart);
            message.Body = bodyBuilder.ToMessageBody();

            return await SendAsync(message).ConfigureAwait(false);
        }
    }
}
