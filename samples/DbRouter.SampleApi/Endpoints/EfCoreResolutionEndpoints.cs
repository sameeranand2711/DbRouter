using System.Data.Common;
using DbRouter.EntityFrameworkCore.Abstractions.Resolvers;
using DbRouter.SampleApi.Configuration;
using DbRouter.SampleApi.Data.Contexts;
using DbRouter.SampleApi.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DbRouter.SampleApi.Endpoints;

public static class EfCoreResolutionEndpoints
{
    public static IEndpointRouteBuilder MapEfCoreResolutionEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/ef");
        group.MapGet("/orders", HandleOrdersAsync);
        group.MapGet("/customers/scoped", HandleScopedCustomersAsync);
        return endpoints;
    }

    private static async Task<IResult> HandleOrdersAsync(
        IDbContextResolver<DatabaseKey, OrdersDbContext> resolver,
        CancellationToken cancellationToken)
    {
        try
        {
            await using OrdersDbContext context = resolver.Create(DatabaseKey.Orders);
            int count = await context.Orders.CountAsync(cancellationToken);
            return Results.Ok(new { Database = DatabaseKey.Orders.ToString(), Count = count });
        }
        catch (DbException)
        {
            return Unavailable(DatabaseKey.Orders);
        }
    }

    private static async Task<IResult> HandleScopedCustomersAsync(
        CustomerRepository customers,
        CustomerPreferenceRepository preferences,
        CancellationToken cancellationToken)
    {
        try
        {
            int customerCount = await customers.CountAsync(cancellationToken);
            int preferenceCount = await preferences.CountAsync(cancellationToken);

            return Results.Ok(new
            {
                Database = DatabaseKey.Customer.ToString(),
                Customers = customerCount,
                Preferences = preferenceCount,
                Resolution = "Scoped CustomerDbContext through constructor DI",
            });
        }
        catch (DbException)
        {
            return Unavailable(DatabaseKey.Customer);
        }
    }

    private static IResult Unavailable(DatabaseKey key) => Results.Problem(
        title: "Sample database unavailable",
        detail: $"Start the configured {key} database before invoking this endpoint.",
        statusCode: StatusCodes.Status503ServiceUnavailable);
}
