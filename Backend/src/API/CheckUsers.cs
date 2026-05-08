using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;

public class UserChecker
{
    public static async System.Threading.Tasks.Task Run(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));
        services.AddScoped<ICurrentUserService, MockCurrentUserService>();

        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = await context.Users.IgnoreQueryFilters().ToListAsync();

            Console.WriteLine($"Found {users.Count} users:");
            foreach (var user in users)
            {
                Console.WriteLine($"- Email: {user.Email}, Role: {user.Role}, IsActive: {user.IsActive}");
            }
        }
    }
}

public class MockCurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;
    public string? Email => null;
    public string? Role => null;
    public Guid? CompanyId => null;
    public bool IsAuthenticated => false;
}
