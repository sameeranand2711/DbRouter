using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DbRouter.Provider.Tests;

public sealed class ProviderConnectionTests
{
    private const string SqlServerConnectionString =
        "Server=localhost;Database=app;User ID=test;Password=sql-secret;Encrypt=False";

    private const string PostgreSqlConnectionString =
        "Host=localhost;Database=reports;Username=test;Password=postgres-secret";

    [Fact]
    public void Mixed_provider_definitions_create_correct_closed_connection_types()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddSqlServer(DatabaseKey.Primary, SqlServerConnectionString);
            builder.AddPostgreSql(DatabaseKey.Reporting, PostgreSqlConnectionString);
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        IDbConnectionFactory<DatabaseKey> factory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();

        using DbConnection primary = factory.Create(DatabaseKey.Primary);
        using DbConnection reporting = factory.Create(DatabaseKey.Reporting);

        Assert.IsType<SqlConnection>(primary);
        Assert.IsType<NpgsqlConnection>(reporting);
        Assert.Equal(ConnectionState.Closed, primary.State);
        Assert.Equal(ConnectionState.Closed, reporting.State);
    }

    [Fact]
    public void Repeated_sql_server_extension_registers_one_provider_for_multiple_keys()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddSqlServer(DatabaseKey.Primary, SqlServerConnectionString);
            builder.AddSqlServer(DatabaseKey.Reporting, SqlServerConnectionString);
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        IDbConnectionFactory<DatabaseKey> factory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();

        using DbConnection primary = factory.Create(DatabaseKey.Primary);
        using DbConnection reporting = factory.Create(DatabaseKey.Reporting);

        Assert.IsType<SqlConnection>(primary);
        Assert.IsType<SqlConnection>(reporting);
    }

    [Fact]
    public void Scoped_selection_works_with_concrete_provider()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
            builder.AddPostgreSql(DatabaseKey.Reporting, PostgreSqlConnectionString));
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<IDatabaseSelection<DatabaseKey>>()
            .Select(DatabaseKey.Reporting);

        using DbConnection connection = scope.ServiceProvider
            .GetRequiredService<IDbConnectionFactory<DatabaseKey>>()
            .Create();

        Assert.IsType<NpgsqlConnection>(connection);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public void Provider_identifiers_are_stable()
    {
        Assert.Equal("sqlserver", new SqlServerDbConnectionProvider().ProviderId);
        Assert.Equal("postgresql", new PostgreSqlDbConnectionProvider().ProviderId);
    }

    [Fact]
    public void Connections_are_naturally_ado_net_and_dapper_compatible()
    {
        IDbConnectionProvider provider = new PostgreSqlDbConnectionProvider();

        using DbConnection connection = provider.Create(PostgreSqlConnectionString);

        Assert.IsAssignableFrom<System.Data.IDbConnection>(connection);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Theory]
    [InlineData("sqlserver")]
    [InlineData("postgresql")]
    public void Direct_provider_construction_failure_does_not_expose_connection_string(
        string providerId)
    {
        const string invalidConnectionString =
            "Definitely Invalid Keyword=value;Password=direct-provider-secret";
        IDbConnectionProvider provider = providerId == "sqlserver"
            ? new SqlServerDbConnectionProvider()
            : new PostgreSqlDbConnectionProvider();

        DbConnectionCreationException exception = Assert.Throws<DbConnectionCreationException>(
            () => provider.Create(invalidConnectionString));

        Assert.DoesNotContain(invalidConnectionString, exception.ToString());
        Assert.DoesNotContain("direct-provider-secret", exception.ToString());
        Assert.Null(exception.InnerException);
    }
}
