using System.Data.Common;
using DbRouter.Core.Abstractions.Connections;
using DbRouter.Core.Abstractions.Resolvers;
using DbRouter.SampleApi.Configuration;
using DbRouter.SampleApi.Models;

namespace DbRouter.SampleApi.Services;

public sealed class DatabaseConnectivityService
{
    private readonly IDbConnectionFactory<DatabaseKey> _connectionFactory;
    private readonly IDbRouter<DatabaseKey> _router;

    public DatabaseConnectivityService(
        IDbConnectionFactory<DatabaseKey> connectionFactory,
        IDbRouter<DatabaseKey> router)
    {
        _connectionFactory = connectionFactory;
        _router = router;
    }

    public async Task<IReadOnlyCollection<DatabaseProbeResult>> ProbeAllAsync(
        CancellationToken cancellationToken)
    {
        Task<DatabaseProbeResult>[] probes = Enum.GetValues<DatabaseKey>()
            .Select(key => ProbeAsync(key, cancellationToken))
            .ToArray();

        return await Task.WhenAll(probes);
    }

    private async Task<DatabaseProbeResult> ProbeAsync(
        DatabaseKey key,
        CancellationToken cancellationToken)
    {
        string provider = _router.Resolve(key).ProviderId;
        string connectionType = "Unavailable";

        try
        {
            await using DbConnection connection = _connectionFactory.Create(key);
            connectionType = connection.GetType().Name;
            await connection.OpenAsync(cancellationToken);
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            object? value = await command.ExecuteScalarAsync(cancellationToken);

            return new DatabaseProbeResult(
                key.ToString(),
                provider,
                connectionType,
                Available: true,
                Convert.ToInt32(value));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new DatabaseProbeResult(
                key.ToString(),
                provider,
                connectionType,
                Available: false,
                ProbeValue: null);
        }
    }
}
