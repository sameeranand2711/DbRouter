using System.Data.Common;

namespace DbRouter.DependencyInjection.Tests;

internal sealed class ThrowingStateDbConnectionProvider : IDbConnectionProvider
{
    public const string Id = "throwing-state";

    public string ProviderId => Id;

    public ThrowingStateDbConnection? LastConnection { get; private set; }

    public DbConnection Create(string connectionString)
    {
        LastConnection = new ThrowingStateDbConnection
        {
            ConnectionString = connectionString,
        };
        return LastConnection;
    }
}
