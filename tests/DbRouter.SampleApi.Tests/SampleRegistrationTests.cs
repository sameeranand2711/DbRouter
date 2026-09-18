using System.Data;
using DbRouter.Core.Abstractions.Connections;
using DbRouter.Core.Abstractions.Providers;
using DbRouter.Core.Abstractions.Resolvers;
using DbRouter.Core.Abstractions.Scoping;
using DbRouter.Core.Models;
using DbRouter.EntityFrameworkCore.Abstractions.Resolvers;
using DbRouter.PostgreSql.Providers;
using DbRouter.SampleApi.Configuration;
using DbRouter.SampleApi.Data.Contexts;
using DbRouter.SampleApi.Data.Entities;
using DbRouter.SampleApi.Data.Repositories;
using DbRouter.SampleApi.Extensions;
using DbRouter.SampleApi.Services;
using DbRouter.SqlServer.Providers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DbRouter.SampleApi.Tests;

public sealed class SampleRegistrationTests
{
    [Fact]
    public void All_seven_database_keys_resolve_to_expected_providers()
    {
        using ServiceProvider provider = SampleTestServices.Build();
        IDbRouter<DatabaseKey> router = provider.GetRequiredService<IDbRouter<DatabaseKey>>();
        var expected = new Dictionary<DatabaseKey, string>
        {
            [DatabaseKey.Primary] = SqlServerDbConnectionProvider.Id,
            [DatabaseKey.Customer] = SqlServerDbConnectionProvider.Id,
            [DatabaseKey.Orders] = SqlServerDbConnectionProvider.Id,
            [DatabaseKey.Payments] = PostgreSqlDbConnectionProvider.Id,
            [DatabaseKey.Inventory] = PostgreSqlDbConnectionProvider.Id,
            [DatabaseKey.Reporting] = SqlServerDbConnectionProvider.Id,
            [DatabaseKey.Audit] = PostgreSqlDbConnectionProvider.Id,
        };

        Assert.Equal(7, Enum.GetValues<DatabaseKey>().Length);
        Assert.All(expected, item => Assert.Equal(item.Value, router.Resolve(item.Key).ProviderId));
    }

    [Fact]
    public void Custom_definition_provider_returns_one_stable_static_snapshot()
    {
        using ServiceProvider provider = SampleTestServices.Build();
        IDatabaseDefinitionProvider<DatabaseKey> definitions = provider
            .GetRequiredService<IDatabaseDefinitionProvider<DatabaseKey>>();

        var sampleProvider = Assert.IsType<SampleDatabaseDefinitionProvider>(definitions);
        IReadOnlyCollection<DatabaseDefinition<DatabaseKey>> first =
            sampleProvider.GetDefinitions();
        IReadOnlyCollection<DatabaseDefinition<DatabaseKey>> second =
            sampleProvider.GetDefinitions();

        Assert.Same(first, second);
        Assert.Equal(7, first.Count);
    }

    [Fact]
    public void Sample_database_options_do_not_render_connection_strings()
    {
        using ServiceProvider provider = SampleTestServices.Build();
        SampleDatabaseOptions options = provider.GetRequiredService<SampleDatabaseOptions>();

        string text = options.ToString();

        Assert.Contains("REDACTED", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Server=", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Host=", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Explicit_keys_create_expected_connection_types_without_opening_them()
    {
        using ServiceProvider provider = SampleTestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        IDbConnectionFactory<DatabaseKey> factory = scope.ServiceProvider
            .GetRequiredService<IDbConnectionFactory<DatabaseKey>>();

        using var orders = factory.Create(DatabaseKey.Orders);
        using var payments = factory.Create(DatabaseKey.Payments);

        Assert.IsType<SqlConnection>(orders);
        Assert.IsType<NpgsqlConnection>(payments);
        Assert.Equal(ConnectionState.Closed, orders.State);
        Assert.Equal(ConnectionState.Closed, payments.State);
    }

    [Fact]
    public void Separate_scopes_hold_independent_database_selections()
    {
        using ServiceProvider provider = SampleTestServices.Build();
        using IServiceScope customerScope = provider.CreateScope();
        using IServiceScope ordersScope = provider.CreateScope();
        IDatabaseSelection<DatabaseKey> customerSelection = customerScope.ServiceProvider
            .GetRequiredService<IDatabaseSelection<DatabaseKey>>();
        IDatabaseSelection<DatabaseKey> ordersSelection = ordersScope.ServiceProvider
            .GetRequiredService<IDatabaseSelection<DatabaseKey>>();

        customerSelection.Select(DatabaseKey.Customer);
        ordersSelection.Select(DatabaseKey.Orders);

        Assert.Equal(DatabaseKey.Customer, customerSelection.SelectedKey);
        Assert.Equal(DatabaseKey.Orders, ordersSelection.SelectedKey);
    }

    [Fact]
    public void Scoped_customer_context_is_configured_after_selection()
    {
        using ServiceProvider provider = SampleTestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IDatabaseSelection<DatabaseKey>>()
            .Select(DatabaseKey.Customer);

        CustomerDbContext context = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);
    }

    [Fact]
    public void Repositories_share_the_same_scoped_context_unit_of_work()
    {
        using ServiceProvider provider = SampleTestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IDatabaseSelection<DatabaseKey>>()
            .Select(DatabaseKey.Customer);
        CustomerRepository customers = scope.ServiceProvider.GetRequiredService<CustomerRepository>();
        CustomerPreferenceRepository preferences = scope.ServiceProvider
            .GetRequiredService<CustomerPreferenceRepository>();
        CustomerDbContext context = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        customers.Add(new Customer { Id = 1, Name = "Sample customer" });
        preferences.Add(new CustomerPreference
        {
            Id = 1,
            CustomerId = 1,
            Preference = "Sample preference",
        });

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [Fact]
    public void Multi_database_service_creates_both_connections_without_changing_selection()
    {
        using ServiceProvider provider = SampleTestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        IDatabaseSelection<DatabaseKey> selection = scope.ServiceProvider
            .GetRequiredService<IDatabaseSelection<DatabaseKey>>();
        selection.Select(DatabaseKey.Customer);
        ExplicitConnectionExampleService service = scope.ServiceProvider
            .GetRequiredService<ExplicitConnectionExampleService>();

        var connections = service.CreateOrdersAndPayments();
        using var orders = connections.Orders;
        using var payments = connections.Payments;

        Assert.IsType<SqlConnection>(orders);
        Assert.IsType<NpgsqlConnection>(payments);
        Assert.Equal(DatabaseKey.Customer, selection.SelectedKey);
    }

    [Fact]
    public async Task Explicit_orders_context_is_caller_owned_and_sql_server_configured()
    {
        using ServiceProvider provider = SampleTestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        IDbContextResolver<DatabaseKey, OrdersDbContext> resolver = scope.ServiceProvider
            .GetRequiredService<IDbContextResolver<DatabaseKey, OrdersDbContext>>();

        await using OrdersDbContext context = resolver.Create(DatabaseKey.Orders);

        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);
    }

    [Fact]
    public async Task Customer_seed_repairs_an_existing_customer_without_a_preference()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new CustomerDbContext(options);
        context.Customers.Add(new Customer { Name = "DbRouter sample customer" });
        await context.SaveChangesAsync();

        await ApplicationInitializationExtensions.SeedCustomersAsync(context, default);
        await ApplicationInitializationExtensions.SeedCustomersAsync(context, default);

        Assert.Single(context.Customers);
        Assert.Single(context.CustomerPreferences);
    }
}
