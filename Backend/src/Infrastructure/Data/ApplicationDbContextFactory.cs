using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Application.Interfaces;

namespace Infrastructure.Data
{
    /// <summary>
    /// Implémentation factice (no-op) de ICurrentUserService pour le temps de conception (EF Migrations).
    /// Le rôle SuperAdmin garantit le contournement de tous les filtres de requête globaux de multi-tenancy lors des migrations.
    /// </summary>
    internal class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;
        public Guid? CompanyId => null;
        public string? Role => "SuperAdmin";
        public bool IsAuthenticated => false;
    }

    /// <summary>
    /// Fabrique de contexte de base de données utilisée en temps de conception par les outils de ligne de commande d'Entity Framework Core.
    /// Lit la chaîne de connexion depuis la variable d'environnement DB_CONNECTION ou utilise une valeur par défaut de développement.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        /// <summary>
        /// Crée une nouvelle instance de <see cref="ApplicationDbContext"/> pour le design-time.
        /// </summary>
        /// <param name="args">Arguments passés par les outils de CLI d'EF Core.</param>
        /// <returns>Une instance configurée de <see cref="ApplicationDbContext"/>.</returns>
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var conn = Environment.GetEnvironmentVariable("DB_CONNECTION")
                       ?? "Host=localhost;Port=5433;Database=novahiredb2;Username=postgres;Password=root";

            builder.UseNpgsql(conn);
            builder.ConfigureWarnings(w => {
                w.Default(WarningBehavior.Log);
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
            });

            return new ApplicationDbContext(builder.Options, new DesignTimeCurrentUserService());
        }
    }
}
