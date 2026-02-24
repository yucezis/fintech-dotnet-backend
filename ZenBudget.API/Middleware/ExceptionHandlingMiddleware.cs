using System.Net;
using System.Text.Json;
using ZenBudget.Application.DTOs;

namespace ZenBudget.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // İsteği normal akışına bırak
            await _next(context);
        }
        catch (Exception ex)
        {
            // Bir hata fırlatılırsa burada yakala, logla ve standart yanıt dön
            _logger.LogError(ex, "Sistemde beklenmeyen bir hata oluştu.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = ApiResponse<object>.ErrorResponse(
            "Sunucu hatası: " + exception.Message,
            context.Response.StatusCode
        );

        // Yanıtı CamelCase (Flutter'ın sevdiği format) olarak serileştir
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return context.Response.WriteAsync(json);
    }
}