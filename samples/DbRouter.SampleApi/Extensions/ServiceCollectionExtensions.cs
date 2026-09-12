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

        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddSqlServer(
                DatabaseKey.Primary,
                ConnectionString("PrimaryDatabase"));
            builder.AddSqlServer(
                DatabaseKey.Customer,
                ConnectionString("CustomerDatabase"));
            builder.AddSqlServer(
                DatabaseKey.Orders,
                ConnectionString("OrdersDatabase"));
            builder.AddPostgreSql(
                DatabaseKey.Payments,
                ConnectionString("PaymentsDatabase"));
            builder.AddPostgreSql(
                DatabaseKey.Inventory,
                ConnectionString("InventoryDatabase"));
            builder.AddSqlServer(
                DatabaseKey.Reporting,
                ConnectionString("ReportingDatabase"));
            builder.AddPostgreSql(
                DatabaseKey.Audit,
                ConnectionString("AuditDatabase"));
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

        return services;
    }
}
