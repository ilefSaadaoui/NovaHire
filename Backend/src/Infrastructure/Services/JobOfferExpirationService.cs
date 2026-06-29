using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    /// <summary>
    /// Service d'arrière-plan (Background Service) chargé d'expirer automatiquement les offres d'emploi publiées.
    /// S'exécute de manière autonome toutes les heures en tant que <see cref="BackgroundService"/> hébergé par l'ASP.NET Host.
    /// Utilise un scope de service temporaire (Scoped) pour accéder au DbContext EF Core en toute sécurité depuis un Singleton.
    /// </summary>
    public class JobOfferExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobOfferExpirationService> _logger;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="JobOfferExpirationService"/>.
        /// </summary>
        /// <param name="serviceProvider">Le fournisseur de services DI pour créer des scopes temporaires.</param>
        /// <param name="logger">Le logger pour tracer les cycles d'exécution et les offres expirées.</param>
        public JobOfferExpirationService(IServiceProvider serviceProvider, ILogger<JobOfferExpirationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Boucle principale du service d'arrière-plan.
        /// Vérifie et ferme les offres expirées toutes les heures jusqu'à l'arrêt de l'application.
        /// </summary>
        /// <param name="stoppingToken">Jeton d'annulation signalé lors de l'arrêt de l'application.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("JobOfferExpirationService démarré.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndCloseExpiredOffersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'exécution de JobOfferExpirationService");
                }

                // Pause d'une heure entre chaque cycle de vérification
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        /// <summary>
        /// Interroge la base de données pour trouver toutes les offres d'emploi publiées dont la date d'expiration est dépassée,
        /// puis les clôture automatiquement en mettant leur statut à <see cref="JobOfferStatus.Closed"/>.
        /// </summary>
        /// <param name="stoppingToken">Jeton d'annulation pour interrompre la requête si l'application s'arrête.</param>
        private async Task CheckAndCloseExpiredOffersAsync(CancellationToken stoppingToken)
        {
            // Création d'un scope DI temporaire pour accéder au DbContext (Scoped) depuis ce Singleton
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Contournement du filtre global de multitenancy (IgnoreQueryFilters)
            // car ce service système doit traiter les offres de TOUTES les entreprises simultanément
            var expiredOffers = await dbContext.JobOffers!
                .IgnoreQueryFilters()
                .Where(o => o.Status == JobOfferStatus.Published && o.Deadline.HasValue && o.Deadline.Value < DateTime.UtcNow)
                .ToListAsync(stoppingToken);

            if (expiredOffers.Any())
            {
                _logger.LogInformation($"Fermeture automatique de {expiredOffers.Count} offre(s) expirée(s).");

                foreach (var offer in expiredOffers)
                {
                    offer.Status = JobOfferStatus.Closed;
                    offer.UpdatedAt = DateTime.UtcNow;
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
