using DbRouter.SampleApi.Configuration;

namespace DbRouter.SampleApi.Endpoints;

public static class DatabaseEndpoints
{
    public static IEndpointRouteBuilder MapDatabaseEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/databases",
            () => Results.Ok(Enum.GetNames<DatabaseKey>()));

        return endpoints;
    }
}
