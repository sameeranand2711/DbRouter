using System.Diagnostics;
using DbRouter.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace DbRouter.Benchmarks;

internal static class Program
{
    private const string ConnectionString =
        "Server=localhost;Database=benchmark;Integrated Security=True;Encrypt=False";

    public static void Main()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
            builder.AddSqlServer(DatabaseKey.Primary, ConnectionString));
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        IDbRouter<DatabaseKey> router =
            scope.ServiceProvider.GetRequiredService<IDbRouter<DatabaseKey>>();
        IDatabaseSelection<DatabaseKey> selection =
            scope.ServiceProvider.GetRequiredService<IDatabaseSelection<DatabaseKey>>();
        IDbConnectionFactory<DatabaseKey> factory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();
        selection.Select(DatabaseKey.Primary);

        for (var index = 0; index < 10_000; index++)
        {
            _ = router.Resolve(DatabaseKey.Primary);
            _ = selection.SelectedKey;
            factory.Create(DatabaseKey.Primary).Dispose();
        }

        Measure(
            "key resolution",
            5_000_000,
            _ => GC.KeepAlive(router.Resolve(DatabaseKey.Primary)));
        Measure(
            "scoped selection lookup",
            5_000_000,
            _ => GC.KeepAlive(selection.SelectedKey));
        Measure(
            "provider lookup + SqlConnection construction/disposal",
            100_000,
            _ => factory.Create(DatabaseKey.Primary).Dispose());
    }

    private static void Measure(string name, int operations, Action<int> operation)
    {
        long start = Stopwatch.GetTimestamp();

        for (var index = 0; index < operations; index++)
        {
            operation(index);
        }

        long elapsed = Stopwatch.GetTimestamp() - start;
        double nanosecondsPerOperation =
            elapsed * (1_000_000_000d / Stopwatch.Frequency) / operations;
        Console.WriteLine($"{name}: {nanosecondsPerOperation:N2} ns/op ({operations:N0} operations)");
    }
}
