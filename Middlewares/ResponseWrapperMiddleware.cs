using System.Text.Json;
using System.Text;

namespace Api.Middlewares
{
    public class ResponseWrapperMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseWrapperMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context); // Ejecuta el pipeline

        memoryStream.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(memoryStream).ReadToEndAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);

        // Evita formatear si es swagger, archivos, etc.
        if (context.Response.ContentType?.Contains("application/json") == true &&
            !string.IsNullOrWhiteSpace(body))
        {
            var wrapped = JsonSerializer.Serialize(new
            {
                success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                data = JsonSerializer.Deserialize<object>(body),
                statusCode = context.Response.StatusCode
            });

            context.Response.Body = originalBodyStream;
            context.Response.ContentLength = Encoding.UTF8.GetByteCount(wrapped);
            await context.Response.WriteAsync(wrapped);
        }
        else
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
        }
    }
}

}