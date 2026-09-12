using System.Data.Common;

namespace DbRouter.DependencyInjection.Tests;

internal sealed class ThrowingDbConnectionProvider : IDbConnectionProvider
{
    public const string Id = "throwing";

    public string ProviderId => Id;

    public DbConnection Create(string connectionString) =>
        throw new InvalidOperationException($"Cannot construct connection for {connectionString}");
}
