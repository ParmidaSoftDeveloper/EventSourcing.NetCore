using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Core.WebApi.Tracing.Correlation;

// Inspired by great Steve Gordon's work: https://github.com/stevejgordon/CorrelationId
// https://code-maze.com/dependency-injection-lifetimes-aspnet-core/
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0#service-lifetimes
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string LoggerScopeKey = "Correlation-ID";

    private readonly RequestDelegate next;
    private readonly ILogger<CorrelationIdMiddleware> logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        this.next = next ?? throw new ArgumentNullException(nameof(next));
        this.logger = logger;
    }

    public async Task Invoke(HttpContext context, Func<CorrelationId> funcCorrelation)
    {
        // get correlation id from header or generate a new one
        context.TraceIdentifier =
            context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId)
                ? correlationId
                : funcCorrelation().Value;


        // apply the correlation ID to the response header for client side tracking
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Add(CorrelationIdHeaderName, new[] { context.TraceIdentifier });
            return Task.CompletedTask;
        });

        // add CorrelationId explicitly to the logger scope
        using var scope = logger.BeginScope(new Dictionary<string, object>
               {
                   [LoggerScopeKey] = correlationId
               });

        await next(context);
    }
}

public static class CorrelationIdMiddlewareConfig
{
    public static IServiceCollection AddCorrelationIdMiddleware(this IServiceCollection services)
    {
        services.TryAddScoped<ICorrelationIdFactory, GuidCorrelationIdFactory>();
        services.TryAddScoped<Func<CorrelationId>>(sp => sp.GetRequiredService<ICorrelationIdFactory>().New);

        return services;
    }

    public static IApplicationBuilder UseCorrelationIdMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}
