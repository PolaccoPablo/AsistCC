using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace SaasACCAPI.api.Middleware;

public class JwtAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthorizationMiddleware> _logger;

    public JwtAuthorizationMiddleware(RequestDelegate next, ILogger<JwtAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Verificar si la ruta requiere autenticación
        var endpoint = context.GetEndpoint();
        var authorizeAttribute = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>();
        
        if (authorizeAttribute != null)
        {
            // Verificar si el usuario está autenticado
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Acceso no autorizado a {Path}", context.Request.Path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            // Log de acceso autorizado
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation("Acceso autorizado para usuario {UserId} ({Email}) a {Path}", 
                userId, userEmail, context.Request.Path);
        }

        await _next(context);
    }
}

// Extension method para registrar el middleware
public static class JwtAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtAuthorizationMiddleware>();
    }
}
