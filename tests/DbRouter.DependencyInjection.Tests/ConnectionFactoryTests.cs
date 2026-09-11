using System.Data;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DbRouter.DependencyInjection.Tests;

public sealed class ConnectionFactoryTests
{
    private const string SecretConnectionString = "Host=fake;Password=never-expose-this";

    [Fact]
    public void Explicit_creation_returns_closed_caller_owned_connection()
    {
        var fakeProvider = new FakeDbConnectionProvider();
        using ServiceProvider provider = BuildProvider(fakeProvider);
        using IServiceScope scope = provider.CreateScope();
        IDbConnectionFactory<DatabaseKey> factory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();

        DbConnection connection = factory.Create(DatabaseKey.Primary);

        var fakeConnection = Assert.IsType<FakeDbConnection>(connection);
        Assert.Equal(ConnectionState.Closed, fakeConnection.State);
        Assert.False(fakeConnection.IsDisposed);

        connection.Dispose();

        Assert.True(fakeConnection.IsDisposed);
    }

    [Fact]
    public void Scoped_creation_uses_selected_database()
    {
        using ServiceProvider provider = BuildProvider(new FakeDbConnectionProvider());
        using IServiceScope scope = provider.CreateScope();
        IDatabaseSelection<DatabaseKey> selection =
            scope.ServiceProvider.GetRequiredService<IDatabaseSelection<DatabaseKey>>();
        IDbConnectionFactory<DatabaseKey> factory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();
        selection.Select(DatabaseKey.Reporting);

        using DbConnection connection = factory.Create();

        Assert.Equal("reporting-value", connection.ConnectionString);
    }

    [Fact]
    public void Scoped_creation_requires_selection()
    {
        using ServiceProvider provider = BuildProvider(new FakeDbConnectionProvider());
        using IServiceScope scope = provider.CreateScope();
        IDbConnectionFactory<DatabaseKey> factory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();

        Assert.Throws<DatabaseSelectionMissingException>(() => factory.Create());
    }

    [Fact]
    public void Explicit_creation_does_not_change_scoped_selection()
    {
        using ServiceProvider provider = BuildProvider(new FakeDbConnectionProvider());
        using IServiceScope scope = provider.CreateScope();
        IDatabaseSelection<DatabaseKey> selection =
            scope.ServiceProvider.GetRequiredService<IDatabaseSelection<DatabaseKey>>();
        IDbConnectionFactory<DatabaseKey> factory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();
        selection.Select(DatabaseKey.Primary);

        using DbConnection connection = factory.Create(DatabaseKey.Reporting);

        Assert.Equal("reporting-value", connection.ConnectionString);
        Assert.Equal(DatabaseKey.Primary, selection.SelectedKey);
    }

    [Fact]
    public void Provider_lookup_is_case_insensitive()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddProvider(new FakeDbConnectionProvider("fake"));
            builder.AddDatabase(DatabaseKey.Primary, "FAKE", "safe-test-value");
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        using DbConnection connection = scope.ServiceProvider
            .GetRequiredService<IDbConnectionFactory<DatabaseKey>>()
            .Create(DatabaseKey.Primary);

        Assert.IsType<FakeDbConnection>(connection);
    }

    [Fact]
    public void Missing_provider_fails_without_leaking_connection_string()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
            builder.AddDatabase(DatabaseKey.Primary, "missing", SecretConnectionString));
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        IDbConnectionFactory<DatabaseKey> factory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();

        DbConnectionProviderNotFoundException exception =
            Assert.Throws<DbConnectionProviderNotFoundException>(
                () => factory.Create(DatabaseKey.Primary));

        Assert.DoesNotContain(SecretConnectionString, exception.ToString());
        Assert.DoesNotContain("never-expose-this", exception.ToString());
    }

    [Fact]
    public void Provider_failure_is_sanitized_even_when_provider_echoes_secret()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddProvider(new ThrowingDbConnectionProvider());
            builder.AddDatabase(DatabaseKey.Primary, ThrowingDbConnectionProvider.Id, SecretConnectionString);
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        IDbConnectionFactory<DatabaseKey> factory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();

        DbConnectionCreationException exception = Assert.Throws<DbConnectionCreationException>(
            () => factory.Create(DatabaseKey.Primary));

        Assert.Null(exception.InnerException);
        Assert.DoesNotContain(SecretConnectionString, exception.ToString());
        Assert.DoesNotContain("never-expose-this", exception.ToString());
    }

    [Fact]
    public void Null_connection_from_provider_is_rejected()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddProvider(new NullDbConnectionProvider());
            builder.AddDatabase(DatabaseKey.Primary, NullDbConnectionProvider.Id, SecretConnectionString);
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        Assert.Throws<DbConnectionCreationException>(
            () => scope.ServiceProvider
                .GetRequiredService<IDbConnectionFactory<DatabaseKey>>()
                .Create(DatabaseKey.Primary));
    }

    [Fact]
    public void Open_connection_from_provider_is_disposed_and_rejected()
    {
        var openProvider = new OpenDbConnectionProvider();
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddProvider(openProvider);
            builder.AddDatabase(DatabaseKey.Primary, OpenDbConnectionProvider.Id, SecretConnectionString);
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        Assert.Throws<DbConnectionCreationException>(
            () => scope.ServiceProvider
                .GetRequiredService<IDbConnectionFactory<DatabaseKey>>()
                .Create(DatabaseKey.Primary));
        Assert.NotNull(openProvider.LastConnection);
        Assert.True(openProvider.LastConnection.IsDisposed);
    }

    private static ServiceProvider BuildProvider(FakeDbConnectionProvider fakeProvider)
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddProvider(fakeProvider);
            builder.AddDatabase(DatabaseKey.Primary, fakeProvider.ProviderId, SecretConnectionString);
            builder.AddDatabase(DatabaseKey.Reporting, fakeProvider.ProviderId, "reporting-value");
        });
        return services.BuildServiceProvider();
    }
}
