using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Application.Interfaces;

namespace Infrastructure.Data
{
    /// <summary>
    /// No-op implementation of ICurrentUserService for design-time (EF migrations).
    /// SuperAdmin role ensures all global query filters are bypassed during migrations.
    /// </summary>
    internal class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;
        public Guid? CompanyId => null;
        public string? Role => "SuperAdmin";
        public bool IsAuthenticated => false;
    }

    // Design-time factory for EF Core tools. Reads connection string from DB_CONNECTION env var
    // or falls back to a sensible development default.
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
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
