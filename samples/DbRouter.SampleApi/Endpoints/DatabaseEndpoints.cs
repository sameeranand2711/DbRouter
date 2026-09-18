using DbRouter.Core.Abstractions.Resolvers;
using DbRouter.SampleApi.Configuration;
using DbRouter.SampleApi.Services;

namespace DbRouter.SampleApi.Endpoints;

public static class DatabaseEndpoints
{
    public static IEndpointRouteBuilder MapDatabaseEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/databases",
            (IDbRouter<DatabaseKey> router) => Results.Ok(
                Enum.GetValues<DatabaseKey>()
                    .Select(key => new
                    {
                        Database = key.ToString(),
                        Provider = router.Resolve(key).ProviderId,
                    })));

        endpoints.MapGet(
            "/api/databases/status",
            async (
                DatabaseConnectivityService service,
                CancellationToken cancellationToken) =>
                Results.Ok(await service.ProbeAllAsync(cancellationToken)));

        return endpoints;
    }
}
