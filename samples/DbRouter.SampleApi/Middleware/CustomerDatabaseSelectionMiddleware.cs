using DbRouter.Core.Abstractions.Scoping;
using DbRouter.SampleApi.Configuration;

namespace DbRouter.SampleApi.Middleware;

public sealed class CustomerDatabaseSelectionMiddleware
{
    private readonly RequestDelegate _next;

    public CustomerDatabaseSelectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(
        HttpContext context,
        IDatabaseSelection<DatabaseKey> selection)
    {
        if (context.Request.Path.StartsWithSegments("/api/scoped/customers") ||
            context.Request.Path.StartsWithSegments("/api/ef/customers/scoped"))
        {
            selection.Select(DatabaseKey.Customer);
        }

        return _next(context);
    }
}
