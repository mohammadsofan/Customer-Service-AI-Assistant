using System.Net;
using System.Text.Json;
using AIEmployeeSupport.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AIEmployeeSupport.API.Middleware;

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
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var errorCode = "INTERNAL_ERROR";
        var message = "An unexpected error occurred.";
        object? details = null;

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "VALIDATION_ERROR";
                message = "One or more validation errors occurred.";
                details = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                break;

            case UnauthorizedAccessException unauthEx:
                statusCode = HttpStatusCode.Unauthorized;
                errorCode = "UNAUTHORIZED";
                message = !string.IsNullOrWhiteSpace(unauthEx.Message) && unauthEx.Message != "Attempted to perform an unauthorized operation."
                    ? unauthEx.Message
                    : "بيانات الاعتماد غير صحيحة. يرجى التأكد من البريد وكلمة المرور.";
                break;

            case ForbiddenException forbiddenEx:
                statusCode = HttpStatusCode.Forbidden;
                errorCode = "FORBIDDEN";
                message = forbiddenEx.Message;
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                errorCode = "NOT_FOUND";
                message = notFoundEx.Message;
                break;

            case AIProviderException aiEx:
                statusCode = HttpStatusCode.ServiceUnavailable;
                errorCode = "AI_PROVIDER_UNAVAILABLE";
                message = "The AI provider is currently unavailable. Please try again later.";
                // Do not expose raw AI exception details/keys to client
                break;

            case ArgumentException argEx:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "BAD_REQUEST";
                message = argEx.Message;
                break;

            case InvalidOperationException invEx:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "INVALID_OPERATION";
                message = invEx.Message;
                break;
        }

        var response = new
        {
            Error = errorCode,
            Message = message,
            Details = details
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
