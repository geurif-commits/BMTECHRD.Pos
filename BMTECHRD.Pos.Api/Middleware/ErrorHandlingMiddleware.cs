using System.Net.Mime;
using System.Text.Json;
using BMTECHRD.Pos.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace BMTECHRD.Pos.Api.Middleware;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiProblemException ex)
        {
            await WriteProblemAsync(
                context,
                ex.StatusCode,
                ex.Title,
                ex.Message,
                ex.ErrorCode,
                includeException: null);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                "An unexpected error occurred.",
                "UNEXPECTED_ERROR",
                includeException: _environment.IsDevelopment() ? ex.Message : null);
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int status,
        string title,
        string detail,
        string? code,
        string? includeException)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var pd = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        pd.Extensions["traceId"] = context.TraceIdentifier;
        if (!string.IsNullOrWhiteSpace(code))
            pd.Extensions["code"] = code;
        if (!string.IsNullOrWhiteSpace(includeException))
            pd.Extensions["exception"] = includeException;

        var payload = JsonSerializer.Serialize(pd);
        await context.Response.WriteAsync(payload);
    }
}
