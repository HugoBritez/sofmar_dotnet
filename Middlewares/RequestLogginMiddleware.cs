using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Text;

namespace Api.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = Guid.NewGuid().ToString("N")[..8];
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
            context.Request.EnableBuffering();

            var body = string.Empty;
            if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            var logObject = new
            {
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                Method = context.Request.Method,
                Path = context.Request.Path,
                Query = context.Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString()),
                Headers = context.Request.Headers
                            .Where(h => !h.Key.StartsWith("sec-", StringComparison.OrdinalIgnoreCase))
                            .ToDictionary(h => h.Key, h => h.Value.ToString()),
                Body = body
            };

            Log.Information("Incoming HTTP request {@Request}", logObject);
        }

        private void LogResponse(HttpContext context, string requestId, DateTime startTime)
        {
            var duration = DateTime.UtcNow - startTime;

            Log.Information("HTTP response {@Response}", new
            {
                RequestId = requestId,
                StatusCode = context.Response.StatusCode,
                DurationMs = duration.TotalMilliseconds
            });
        }

        private void LogException(HttpContext context, Exception ex, string requestId)
        {
            var sqlDetails = ex is SqlException sqlEx ? new
            {
                sqlEx.Number,
                sqlEx.State,
                sqlEx.Server,
                sqlEx.Procedure,
                sqlEx.LineNumber
            } : null;

            Log.Error(ex, "Request failed {@Error}", new
            {
                RequestId = requestId,
                ExceptionType = ex.GetType().Name,
                ex.Message,
                Inner = ex.InnerException?.Message,
                Sql = sqlDetails
            });
        }
    }
}
