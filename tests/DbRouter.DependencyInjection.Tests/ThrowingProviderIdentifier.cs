using System.Data.Common;

namespace DbRouter.DependencyInjection.Tests;

internal sealed class ThrowingProviderIdentifier : IDbConnectionProvider
{
    public const string Secret = "provider-identifier-secret";

    public string ProviderId =>
        throw new InvalidOperationException($"Failure containing {Secret}");

    public DbConnection Create(string connectionString) => new FakeDbConnection(connectionString);
}
