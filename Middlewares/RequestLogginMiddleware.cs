// Api/Middleware/RequestLoggingMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Api.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var startTime = DateTime.UtcNow;

            try
            {
                await LogRequest(context, requestId);
                await _next(context);
            }
            catch (Exception ex)
            {
                LogException(context, ex, requestId);
                throw;
            }
            finally
            {
                LogResponse(context, requestId, startTime);
            }
        }

        private async Task LogRequest(HttpContext context, string requestId)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\n[REQUEST {requestId}] {DateTime.Now:HH:mm:ss}");
            sb.AppendLine($"┌─────────────────────────────────────────────────────────────");
            sb.AppendLine($"│ {context.Request.Method} {context.Request.Path}");
            
            // Query Parameters
            if (context.Request.QueryString.HasValue)
            {
                sb.AppendLine($"├─ Query Parameters:");
                foreach (var param in context.Request.Query)
                {
                    sb.AppendLine($"│  • {param.Key}: {param.Value}");
                }
            }

            // Headers
            sb.AppendLine($"├─ Headers:");
            foreach (var header in context.Request.Headers.Where(h => !h.Key.StartsWith("sec-")))
            {
                sb.AppendLine($"│  • {header.Key}: {header.Value}");
            }

            // Body
            if (context.Request.Body.CanRead)
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                if (!string.IsNullOrEmpty(body))
                {
                    sb.AppendLine($"├─ Body:");
                    sb.AppendLine($"│  {body}");
                }
                context.Request.Body.Position = 0;
            }

            sb.AppendLine($"└─────────────────────────────────────────────────────────────");
            _logger.LogInformation(sb.ToString());
        }

        private void LogResponse(HttpContext context, string requestId, DateTime startTime)
        {
            var duration = DateTime.UtcNow - startTime;
            var sb = new StringBuilder();
            sb.AppendLine($"\n[RESPONSE {requestId}] {DateTime.Now:HH:mm:ss}");
            sb.AppendLine($"┌─────────────────────────────────────────────────────────────");
            sb.AppendLine($"│ Status: {context.Response.StatusCode}");
            sb.AppendLine($"│ Duration: {duration.TotalMilliseconds:F2}ms");
            sb.AppendLine($"└─────────────────────────────────────────────────────────────");
            _logger.LogInformation(sb.ToString());
        }

        private void LogException(HttpContext context, Exception ex, string requestId)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\n[ERROR {requestId}] {DateTime.Now:HH:mm:ss}");
            sb.AppendLine($"┌─────────────────────────────────────────────────────────────");
            sb.AppendLine($"│ Exception Type: {ex.GetType().Name}");
            sb.AppendLine($"│ Message: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                sb.AppendLine($"│ Inner Exception: {ex.InnerException.Message}");
            }

            // Detalles específicos para errores SQL
            if (ex is SqlException sqlEx)
            {
                sb.AppendLine($"├─ SQL Error Details:");
                sb.AppendLine($"│  • Error Number: {sqlEx.Number}");
                sb.AppendLine($"│  • State: {sqlEx.State}");
                sb.AppendLine($"│  • Server: {sqlEx.Server}");
                sb.AppendLine($"│  • Procedure: {sqlEx.Procedure}");
                sb.AppendLine($"│  • Line Number: {sqlEx.LineNumber}");
            }

            sb.AppendLine($"└─────────────────────────────────────────────────────────────");
            _logger.LogError(sb.ToString());
        }
    }
}