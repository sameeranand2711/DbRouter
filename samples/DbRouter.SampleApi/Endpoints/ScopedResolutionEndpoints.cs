using System.Data.Common;
using DbRouter.SampleApi.Configuration;
using DbRouter.SampleApi.Services;

namespace DbRouter.SampleApi.Endpoints;

public static class ScopedResolutionEndpoints
{
    public static IEndpointRouteBuilder MapScopedResolutionEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/scoped/customers", HandleCustomersAsync);
        return endpoints;
    }

    private static async Task<IResult> HandleCustomersAsync(
        ScopedConnectionExampleService service,
        CancellationToken cancellationToken)
    {
        try
        {
            int result = await service.ExecuteProbeAsync(cancellationToken);
            return Results.Ok(new
            {
                Database = DatabaseKey.Customer.ToString(),
                Result = result,
                Resolution = "Scoped IDbConnectionFactory",
            });
        }
        catch (DbException)
        {
            return Results.Problem(
                title: "Sample database unavailable",
                detail: "Start the configured Customer database before invoking this endpoint.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
