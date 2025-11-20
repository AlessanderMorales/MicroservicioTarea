using System.Net;
using System.Text.Json;

namespace MicroservicioTarea.Middleware
{
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtAuthenticationMiddleware> _logger;

        public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // Manejar respuestas 401 (No autenticado)
            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
                await HandleUnauthorizedAsync(context);

            // Manejar respuestas 403 (No autorizado)
            if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
                await HandleForbiddenAsync(context);
        }

        private async Task HandleUnauthorizedAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = true,
                message = "Token de autenticación no válido o expirado",
                statusCode = 401,
                details = "Por favor, inicie sesión nuevamente"
            };

            _logger.LogWarning($"Intento de acceso no autenticado a: {context.Request.Path}");

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }

        private async Task HandleForbiddenAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = true,
                message = "No tiene permisos para acceder a este recurso",
                statusCode = 403,
                details = "Acceso denegado"
            };

            _logger.LogWarning($"Acceso denegado a: {context.Request.Path} para usuario autenticado");

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public static class JwtAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtAuthenticationMiddleware>();
        }
    }
}
