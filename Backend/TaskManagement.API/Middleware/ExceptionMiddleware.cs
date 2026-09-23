using System.Net;
using System.Text.Json;
using TaskManagement.API.DTOs;
using TaskManagement.API.Exceptions;

namespace TaskManagement.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;

        switch (exception)
        {
            case AppException appEx:
                statusCode = appEx.StatusCode;
                message = appEx.Message;
                break;
            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                message = "The requested resource was not found.";
                break;
            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Forbidden;
                message = "You do not have permission to perform this action.";
                break;
            case ArgumentException argEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = argEx.Message;
                break;
            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "An unexpected internal server error occurred.";
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = ApiResponse.Fail(message);
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, jsonOptions);

        await context.Response.WriteAsync(json);
    }
}
