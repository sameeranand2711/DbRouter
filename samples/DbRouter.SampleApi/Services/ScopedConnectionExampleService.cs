using DbRouter.Core.Abstractions.Connections;
using DbRouter.SampleApi.Configuration;

namespace DbRouter.SampleApi.Services;

public sealed class ScopedConnectionExampleService
{
    private readonly IDbConnectionFactory<DatabaseKey> _connectionFactory;

    public ScopedConnectionExampleService(
        IDbConnectionFactory<DatabaseKey> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> ExecuteProbeAsync(CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(value);
    }
}
