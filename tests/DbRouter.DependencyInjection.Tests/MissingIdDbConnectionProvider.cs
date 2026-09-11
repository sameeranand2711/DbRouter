using System.Data.Common;

namespace DbRouter.DependencyInjection.Tests;

internal sealed class MissingIdDbConnectionProvider : IDbConnectionProvider
{
    public string ProviderId => " ";

    public DbConnection Create(string connectionString) => new FakeDbConnection(connectionString);
}
