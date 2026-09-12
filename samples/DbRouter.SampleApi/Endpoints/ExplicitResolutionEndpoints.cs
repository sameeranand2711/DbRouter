using System.Data.Common;
using DbRouter.SampleApi.Configuration;
using DbRouter.SampleApi.Data.Repositories;
using DbRouter.SampleApi.Services;

namespace DbRouter.SampleApi.Endpoints;

public static class ExplicitResolutionEndpoints
{
    public static IEndpointRouteBuilder MapExplicitResolutionEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/connections");
        group.MapGet("/orders", HandleOrdersAsync);
        group.MapGet("/orders-and-payments", HandleOrdersAndPaymentsAsync);
        return endpoints;
    }

    private static async Task<IResult> HandleOrdersAsync(
        OrderLookupRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            int result = await repository.ExecuteProbeAsync(cancellationToken);
            return Results.Ok(new { Database = DatabaseKey.Orders.ToString(), Result = result });
        }
        catch (DbException)
        {
            return Unavailable();
        }
    }

    private static async Task<IResult> HandleOrdersAndPaymentsAsync(
        ExplicitConnectionExampleService service,
        CancellationToken cancellationToken)
    {
        try
        {
            (DbConnection ordersConnection, DbConnection paymentsConnection) =
                service.CreateOrdersAndPayments();
            await using DbConnection orders = ordersConnection;
            await using DbConnection payments = paymentsConnection;

            await orders.OpenAsync(cancellationToken);
            await payments.OpenAsync(cancellationToken);
            int ordersResult = await ExecuteProbeAsync(orders, cancellationToken);
            int paymentsResult = await ExecuteProbeAsync(payments, cancellationToken);

            return Results.Ok(new
            {
                Orders = ordersResult,
                Payments = paymentsResult,
            });
        }
        catch (DbException)
        {
            return Unavailable();
        }
    }

    private static async Task<int> ExecuteProbeAsync(
        DbConnection connection,
        CancellationToken cancellationToken)
    {
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(value);
    }

    private static IResult Unavailable() => Results.Problem(
        title: "Sample database unavailable",
        detail: "Start the configured local database before invoking this endpoint.",
        statusCode: StatusCodes.Status503ServiceUnavailable);
}
