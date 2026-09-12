using System.Collections.Concurrent;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DbRouter.DependencyInjection.Tests;

public sealed class ConnectionFactoryConcurrencyTests
{
    [Fact]
    public void Concurrent_connection_creation_is_safe()
    {
        var fakeProvider = new FakeDbConnectionProvider();
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddProvider(fakeProvider);
            builder.AddDatabase(DatabaseKey.Primary, fakeProvider.ProviderId, "safe-test-value");
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        IDbConnectionFactory<DatabaseKey> factory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();
        var failures = new ConcurrentQueue<Exception>();

        Parallel.For(
            0,
            5_000,
            _ =>
            {
                try
                {
                    using DbConnection connection = factory.Create(DatabaseKey.Primary);
                    Assert.IsType<FakeDbConnection>(connection);
                }
                catch (Exception exception)
                {
                    failures.Enqueue(exception);
                }
            });

        Assert.Empty(failures);
        Assert.Equal(5_000, fakeProvider.CreateCount);
    }

    [Fact]
    public void Concurrent_scopes_keep_selections_isolated()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddProvider(new FakeDbConnectionProvider());
            builder.AddDatabase(DatabaseKey.Primary, "fake", "primary-value");
            builder.AddDatabase(DatabaseKey.Reporting, "fake", "reporting-value");
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        var failures = new ConcurrentQueue<Exception>();

        Parallel.For(
            0,
            1_000,
            index =>
            {
                try
                {
                    using IServiceScope scope = provider.CreateScope();
                    DatabaseKey key = index % 2 == 0
                        ? DatabaseKey.Primary
                        : DatabaseKey.Reporting;
                    IDatabaseSelection<DatabaseKey> selection =
                        scope.ServiceProvider.GetRequiredService<IDatabaseSelection<DatabaseKey>>();
                    selection.Select(key);
                    using DbConnection connection = scope.ServiceProvider
                        .GetRequiredService<IDbConnectionFactory<DatabaseKey>>()
                        .Create();
                    Assert.Equal(
                        key == DatabaseKey.Primary ? "primary-value" : "reporting-value",
                        connection.ConnectionString);
                }
                catch (Exception exception)
                {
                    failures.Enqueue(exception);
                }
            });

        Assert.Empty(failures);
    }
}
