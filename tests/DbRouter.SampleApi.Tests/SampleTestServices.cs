using DbRouter.SampleApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbRouter.SampleApi.Tests;

internal static class SampleTestServices
{
    public static ServiceProvider Build()
    {
        var values = new Dictionary<string, string?>
        {
            ["ConnectionStrings:PrimaryDatabase"] = SqlServer("Primary"),
            ["ConnectionStrings:CustomerDatabase"] = SqlServer("Customer"),
            ["ConnectionStrings:OrdersDatabase"] = SqlServer("Orders"),
            ["ConnectionStrings:PaymentsDatabase"] = PostgreSql("payments"),
            ["ConnectionStrings:InventoryDatabase"] = PostgreSql("inventory"),
            ["ConnectionStrings:ReportingDatabase"] = SqlServer("Reporting"),
            ["ConnectionStrings:AuditDatabase"] = PostgreSql("audit"),
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        var services = new ServiceCollection();
        services.AddSampleDataAccess(configuration);
        return services.BuildServiceProvider(validateScopes: true);
    }

    private static string SqlServer(string database) =>
        $"Server=localhost;Database=Sample{database};Integrated Security=true;TrustServerCertificate=true";

    private static string PostgreSql(string database) =>
        $"Host=localhost;Database=sample_{database};Username=sample";
}
