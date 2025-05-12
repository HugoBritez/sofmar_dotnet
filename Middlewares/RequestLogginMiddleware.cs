// Api/Middleware/RequestLoggingMiddleware.cs
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
        // Log del request
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");

        // Capturar el body del request si es necesario
        if (context.Request.Body.CanRead)
        {
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            _logger.LogInformation($"Request Body: {body}");
            context.Request.Body.Position = 0;
        }

        // Procesar el request
        await _next(context);

        // Log del response
        _logger.LogInformation($"Response: {context.Response.StatusCode}");
    }
}