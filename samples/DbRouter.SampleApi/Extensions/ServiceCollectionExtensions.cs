using DbRouter.DependencyInjection.Extensions;
using DbRouter.EntityFrameworkCore.Extensions;
using DbRouter.PostgreSql.Extensions;
using DbRouter.SampleApi.Configuration;
using DbRouter.SampleApi.Data.Contexts;
using DbRouter.SampleApi.Data.Repositories;
using DbRouter.SampleApi.Services;
using DbRouter.SqlServer.Extensions;
using DbRouter.SqlServer.Providers;
using Microsoft.EntityFrameworkCore;

namespace DbRouter.SampleApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSampleDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string ConnectionString(string name) =>
            configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException(
                $"Connection string '{name}' is required by the sample.");

        services.AddSingleton(new SampleDatabaseOptions(
            ConnectionString("PrimaryDatabase"),
            ConnectionString("CustomerDatabase"),
            ConnectionString("OrdersDatabase"),
            ConnectionString("PaymentsDatabase"),
            ConnectionString("InventoryDatabase"),
            ConnectionString("ReportingDatabase"),
            ConnectionString("AuditDatabase")));

        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.UseDefinitionProvider<SampleDatabaseDefinitionProvider>();
            builder.AddSqlServer();
            builder.AddPostgreSql();
        });

        services.AddDbRouterEntityFrameworkCore<DatabaseKey, CustomerDbContext>(
            options => new CustomerDbContext(options),
            builder => builder.AddProvider(
                SqlServerDbConnectionProvider.Id,
                (options, connectionString) => options.UseSqlServer(connectionString)));

        services.AddDbRouterEntityFrameworkCore<DatabaseKey, OrdersDbContext>(
            options => new OrdersDbContext(options),
            builder => builder.AddProvider(
                SqlServerDbConnectionProvider.Id,
                (options, connectionString) => options.UseSqlServer(connectionString)));

        services.AddScoped<CustomerRepository>();
        services.AddScoped<CustomerPreferenceRepository>();
        services.AddScoped<OrderLookupRepository>();
        services.AddScoped<ExplicitConnectionExampleService>();
        services.AddScoped<ScopedConnectionExampleService>();
        services.AddScoped<DatabaseConnectivityService>();

        return services;
    }
}
