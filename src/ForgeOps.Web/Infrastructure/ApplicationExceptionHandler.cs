using System.Net;
using ForgeOps.Web.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ForgeOps.Web.Infrastructure;

public class ApplicationExceptionHandler(ILogger<ApplicationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var status = exception switch { WorkOrderValidationException => 400, WorkOrderNotFoundException => 404, _ => 500 };
        if (status == 500)
            logger.LogError(exception, "Request {Method} {Path} failed. Trace {TraceId}", context.Request.Method, context.Request.Path, context.TraceIdentifier);
        else
            logger.LogWarning("Request {Method} {Path} rejected with {Status}. Trace {TraceId}", context.Request.Method, context.Request.Path, status, context.TraceIdentifier);
        context.Response.StatusCode = status;
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            ProblemDetails problem = exception is WorkOrderValidationException validation
                ? new ValidationProblemDetails(validation.Errors) { Title = "Check the highlighted fields.", Status = status }
                : new ProblemDetails { Title = status == 404 ? "Work order not found." : "Something went wrong. Please try again.", Status = status };
            problem.Extensions["traceId"] = context.TraceIdentifier;
            // Serialize the derived ValidationProblemDetails contract, including its Errors property.
            await context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null,
                contentType: "application/problem+json", cancellationToken: ct);
        }
        else
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync($"<!doctype html><html lang=\"en\"><meta name=\"viewport\" content=\"width=device-width\"><title>ForgeOps — Request failed</title><body style=\"font:18px system-ui;padding:10%;color:#243b45\"><h1>We couldn't complete that request.</h1><p>Please try again. If it continues, check the application log.</p><p>Reference: {WebUtility.HtmlEncode(context.TraceIdentifier)}</p><a href=\"/\">Return to dashboard</a></body></html>", ct);
        }
        return true;
    }
}
