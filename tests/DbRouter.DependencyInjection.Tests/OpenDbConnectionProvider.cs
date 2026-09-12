using System.Data;
using System.Data.Common;

namespace DbRouter.DependencyInjection.Tests;

internal sealed class OpenDbConnectionProvider : IDbConnectionProvider
{
    public const string Id = "open";

    public string ProviderId => Id;

    public FakeDbConnection? LastConnection { get; private set; }

    public DbConnection Create(string connectionString)
    {
        LastConnection = new FakeDbConnection(connectionString, ConnectionState.Open);
        return LastConnection;
    }
}
