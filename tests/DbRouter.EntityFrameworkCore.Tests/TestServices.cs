using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbRouter.EntityFrameworkCore.Tests;

internal static class TestServices
{
    public const string ProviderId = "inmemory";

    public static ServiceProvider Build(
        string? primaryName = null,
        string? reportingName = null)
    {
        primaryName ??= $"primary-{Guid.NewGuid():N}";
        reportingName ??= $"reporting-{Guid.NewGuid():N}";

        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddDatabase(DatabaseKey.Primary, ProviderId, primaryName);
            builder.AddDatabase(DatabaseKey.Reporting, ProviderId, reportingName);
        });
        services.AddDbRouterEntityFrameworkCore<DatabaseKey, TestDbContext>(
            options => new TestDbContext(options),
            builder => builder.AddProvider(
                ProviderId,
                (options, databaseName) => options.UseInMemoryDatabase(databaseName)));
        services.AddScoped<TestRepository>();
        return services.BuildServiceProvider(validateScopes: true);
    }
}
