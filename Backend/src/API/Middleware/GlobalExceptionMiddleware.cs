using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Middleware
{
    /// <summary>
    /// Middleware de gestion globale des exceptions
    /// Centralise le traitement des erreurs pour tous les contrôleurs
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                UnauthorizedAccessException ex => (HttpStatusCode.Unauthorized, ex.Message),
                InvalidOperationException ex => (HttpStatusCode.BadRequest, ex.Message),
                ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message),
                KeyNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
                DbUpdateException ex => (HttpStatusCode.Conflict, ParseDbError(ex)),
                _ => (HttpStatusCode.InternalServerError, "Une erreur interne s'est produite")
            };

            // Log uniquement les erreurs serveur en détail
            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Erreur non gérée: {Message}", exception.Message);
            }
            else
            {
                _logger.LogWarning("Erreur applicative ({StatusCode}): {Message}", (int)statusCode, exception.Message);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = JsonSerializer.Serialize(new
            {
                message,
                detail = statusCode == HttpStatusCode.InternalServerError ? exception.Message : null, // Optional: only for debugging
                stackTrace = statusCode == HttpStatusCode.InternalServerError ? exception.StackTrace : null // Optional: only for debugging
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(response);
        }

        private static string ParseDbError(DbUpdateException ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) || inner.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            {
                return "Un enregistrement avec ces données existe déjà (email ou nom en doublon).";
            }
            return "Erreur lors de la sauvegarde des données. Veuillez réessayer.";
        }
    }
}
