using System.Diagnostics;
using DotnetForge.Application.Common;

namespace DotnetForge.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToApiResult<T>(this Result<T> result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        if (result.Error?.Type == ErrorType.Validation)
        {
            return Results.ValidationProblem(
                errors: result.ValidationErrors ?? new Dictionary<string, string[]>(),
                title: "Validation failed",
                type: "https://datatracker.ietf.org/doc/html/rfc9457",
                extensions: CreateExtensions(httpContext, result.Error));
        }

        var statusCode = result.Error?.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            statusCode: statusCode,
            title: result.Error?.Message ?? "Request failed",
            type: "https://datatracker.ietf.org/doc/html/rfc9457",
            extensions: CreateExtensions(httpContext, result.Error));
    }

    private static Dictionary<string, object?> CreateExtensions(HttpContext httpContext, Error? error) =>
        new()
        {
            ["correlationId"] = httpContext.GetCorrelationId(),
            ["traceId"] = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
            ["errorCode"] = error?.Code
        };
}
