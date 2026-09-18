using DbRouter.Core.Abstractions.Providers;
using DbRouter.Core.Models;
using DbRouter.PostgreSql.Providers;
using DbRouter.SqlServer.Providers;

namespace DbRouter.SampleApi.Configuration;

public sealed class SampleDatabaseDefinitionProvider :
    IDatabaseDefinitionProvider<DatabaseKey>
{
    private readonly IReadOnlyCollection<DatabaseDefinition<DatabaseKey>> _definitions;

    public SampleDatabaseDefinitionProvider(SampleDatabaseOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _definitions =
        [
            new(DatabaseKey.Primary, SqlServerDbConnectionProvider.Id, options.PrimaryConnectionString),
            new(DatabaseKey.Customer, SqlServerDbConnectionProvider.Id, options.CustomerConnectionString),
            new(DatabaseKey.Orders, SqlServerDbConnectionProvider.Id, options.OrdersConnectionString),
            new(DatabaseKey.Payments, PostgreSqlDbConnectionProvider.Id, options.PaymentsConnectionString),
            new(DatabaseKey.Inventory, PostgreSqlDbConnectionProvider.Id, options.InventoryConnectionString),
            new(DatabaseKey.Reporting, SqlServerDbConnectionProvider.Id, options.ReportingConnectionString),
            new(DatabaseKey.Audit, PostgreSqlDbConnectionProvider.Id, options.AuditConnectionString),
        ];
    }

    public IReadOnlyCollection<DatabaseDefinition<DatabaseKey>> GetDefinitions() =>
        _definitions;
}
