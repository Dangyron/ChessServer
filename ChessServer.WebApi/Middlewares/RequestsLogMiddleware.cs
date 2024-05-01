using Serilog.Context;

namespace ChessServer.WebApi.Middlewares;

public sealed class RequestsLogMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
        {
            return next(context);
        }
    }
}