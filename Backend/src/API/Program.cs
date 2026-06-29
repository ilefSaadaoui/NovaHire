using System.Text;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;
using API.Middleware;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Domain.Entities;
using AI;
using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// Avoid startup failure when 5000 is already occupied in local development.
var aspNetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (!string.IsNullOrWhiteSpace(aspNetCoreUrls) &&
    aspNetCoreUrls.Contains("5000") &&
    IsPortInUse(5000))
{
    var fallbackUrls = aspNetCoreUrls
        .Replace("127.0.0.1:5000", "127.0.0.1:5001")
        .Replace("localhost:5000", "localhost:5001");

    builder.WebHost.UseUrls(fallbackUrls);

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"⚠️  WARNING: Port 5000 is in use. Falling back to {fallbackUrls}.");
    Console.ResetColor();
}

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Bind EmailSettings for DI
builder.Services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

// Swagger/OpenAPI with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NovaHire API",
        Version = "v1",
        Description = "API de la plateforme de recrutement SaaS NovaHire"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Exemple: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString("PostgreSQL"));
    options.ConfigureWarnings(w => {
        w.Default(WarningBehavior.Log);
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
    });
});

// JWT Authentication
var jwtSettings = configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

// Validation: reject default/placeholder JWT secret
var defaultSecret = "your-super-secret-key-must-be-at-least-32-characters-long-for-security";
if (string.IsNullOrEmpty(secretKey) || secretKey == defaultSecret)
{
    // Generate a per-machine dev fallback to avoid insecure hardcoded defaults.
    secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
        ?? Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());

    configuration["JwtSettings:SecretKey"] = secretKey;

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("⚠️  WARNING: JWT secret not configured. A temporary development secret was generated. Configure 'JwtSettings:SecretKey' via user-secrets or environment variables for production.");
    Console.ResetColor();
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("CompanyAdminOrAbove", policy => policy.RequireRole("SuperAdmin", "CompanyAdmin"));
    options.AddPolicy("RecruiterOrAbove", policy => policy.RequireRole("SuperAdmin", "CompanyAdmin", "Recruiter"));
});

// CORS — autoriser HTTP et HTTPS depuis localhost (dev) et l'URL frontend configurée (prod)
builder.Services.AddCors(options =>
{
    var frontendUrl = configuration["AppSettings:FrontendUrl"] ?? "https://localhost:3010";
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
              {
                  var host = new Uri(origin).Host;
                  // Accepter localhost (HTTP + HTTPS) en développement
                  if (host == "localhost" || host == "127.0.0.1")
                      return true;
                  // Accepter l'URL de production configurée
                  return origin.StartsWith(frontendUrl, StringComparison.OrdinalIgnoreCase);
              })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Dependency Injection - Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
// Register MailKit-backed email service and ensure EmailSettings are bound
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Dependency Injection - Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IJobOfferRepository, JobOfferRepository>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
// Register AI Service with Polly (Retry + Circuit Breaker + Logging)
builder.Services.AddHttpClient<IAIService, AIAnalysisService>(client => 
{
    client.BaseAddress = new Uri(builder.Configuration["AISettings:PythonServiceUrl"] ?? "http://localhost:8000");
})
.AddPolicyHandler((sp, msg) => 
{
    var logger = sp.GetRequiredService<ILogger<AIAnalysisService>>();
    
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, 
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) => 
            {
                logger.LogWarning("AI SERVICE RETRY: Tentative {Attempt} apr\u00e8s {Duration}s. Motif: {Error}", 
                    retryAttempt, timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
            });
})
.AddPolicyHandler((sp, msg) => 
{
    var logger = sp.GetRequiredService<ILogger<AIAnalysisService>>();
    
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
            onBreak: (result, timespan) => 
                logger.LogCritical("CIRCUIT BREAKER OUVERT pour {Duration}s. Le service IA est d\u00e9faillant. Erreur: {Error}", 
                    timespan.TotalSeconds, result.Exception?.Message ?? result.Result.StatusCode.ToString()),
            onReset: () => logger.LogInformation("CIRCUIT BREAKER FERM\u00c9. Le service IA est de nouveau en ligne."));
});
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IStorageService, CloudinaryService>();

// Register Background Services
builder.Services.AddHostedService<JobOfferExpirationService>();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Build app
var app = builder.Build();

// Global error handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NovaHire API v1");
        c.RoutePrefix = "swagger";
    });
}
// HTTPS Redirection — active en développement ET en production
app.UseHttpsRedirection();

// HSTS — forcer HTTPS sur les navigateurs en production uniquement
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseStaticFiles();

app.UseRouting();

// CORS must be after UseRouting but before UseAuthentication/UseAuthorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Map admin dashboard
app.MapGet("/admin", async (context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/admin.html");
});

app.MapControllers();

// Seed initial data (optional)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Apply migrations automatically
        context.Database.Migrate();

        // Seed data if necessary
        // await SeedData.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database");
    }
}

try
{
    app.Run();
}
catch (Exception ex)
{
    System.IO.File.WriteAllText("crash_log.txt", ex.ToString());
    throw;
}

static bool IsPortInUse(int port)
{
    try
    {
        using var client = new TcpClient();
        client.Connect(IPAddress.Loopback, port);
        return true;
    }
    catch (SocketException)
    {
        return false;
    }
}