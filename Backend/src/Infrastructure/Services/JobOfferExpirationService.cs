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
    public class JobOfferExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobOfferExpirationService> _logger;

        public JobOfferExpirationService(IServiceProvider serviceProvider, ILogger<JobOfferExpirationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

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

                // Vérifier toutes les heures
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CheckAndCloseExpiredOffersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // On ignore les filtres de locataires car c'est un service système global
            var expiredOffers = await dbContext.JobOffers!
                .IgnoreQueryFilters()
                .Where(o => o.Status == JobOfferStatus.Published && o.ExpiresAt.HasValue && o.ExpiresAt.Value < DateTime.UtcNow)
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
