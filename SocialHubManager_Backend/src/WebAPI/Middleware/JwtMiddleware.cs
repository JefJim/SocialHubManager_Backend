using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            var method = context.Request.Method;

            // Loguear solicitud
            _logger.LogInformation("Request {Method} {Path}", method, path);

            // Obtener token del header Authorization
            var tokenHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            string? token = null;

            if (!string.IsNullOrEmpty(tokenHeader) && tokenHeader.StartsWith("Bearer "))
            {
                token = tokenHeader.Substring("Bearer ".Length).Trim();
            }

            if (!string.IsNullOrEmpty(token))
            {
            }
            else
            {
                _logger.LogWarning("No se encontró JWT en la solicitud.");
            }

            // Loguear claims (si hay usuario autenticado)
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                foreach (var claim in context.User.Claims)
                {
                }
            }
            else
            {
                _logger.LogInformation("Usuario no autenticado en este request.");
            }

            await _next(context);
        }
    }
}
