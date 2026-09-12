using System.Data.Common;

namespace DbRouter.DependencyInjection.Tests;

internal sealed class FakeDbConnectionProvider : IDbConnectionProvider
{
    private int _createCount;

    public FakeDbConnectionProvider(string providerId = "fake")
    {
        ProviderId = providerId;
    }

    public string ProviderId { get; }

    public int CreateCount => Volatile.Read(ref _createCount);

    public FakeDbConnection? LastConnection { get; private set; }

    public DbConnection Create(string connectionString)
    {
        Interlocked.Increment(ref _createCount);
        var connection = new FakeDbConnection(connectionString);
        LastConnection = connection;
        return connection;
    }
}
