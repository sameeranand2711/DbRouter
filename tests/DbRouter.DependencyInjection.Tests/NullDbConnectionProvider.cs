using System.Data.Common;

namespace DbRouter.DependencyInjection.Tests;

internal sealed class NullDbConnectionProvider : IDbConnectionProvider
{
    public const string Id = "null";

    public string ProviderId => Id;

    public DbConnection Create(string connectionString) => null!;
}
