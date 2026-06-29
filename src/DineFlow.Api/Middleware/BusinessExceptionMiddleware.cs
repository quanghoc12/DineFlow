using System.Text.Json;
using DineFlow.Services.Common;

namespace DineFlow.Api.Middleware;

public class BusinessExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public BusinessExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                code = ex.Code,
                message = ex.Message
            }));
        }
    }
}
