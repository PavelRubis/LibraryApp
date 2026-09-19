using Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Validation failed", exception.Message, exception.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(error => error.ErrorMessage).ToArray()));
        }
        catch (NotFoundException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Not found", exception.Message);
        }
        catch (ConflictException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflict", exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled request error for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.");
        }
    }

    private static Task WriteProblemAsync(
        HttpContext context,
        int status,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        context.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        return context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: context.RequestAborted);
    }
}
