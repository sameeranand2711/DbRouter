using System.Collections.Concurrent;

namespace DbRouter.Core.Tests;

public sealed class DbRouterConcurrencyTests
{
    [Fact]
    public void Concurrent_resolution_is_safe()
    {
        DbRouter<DatabaseKey> router = TestDefinitions.CreateRouter();
        var failures = new ConcurrentQueue<Exception>();

        Parallel.For(
            0,
            20_000,
            index =>
            {
                try
                {
                    DatabaseKey key = index % 2 == 0
                        ? DatabaseKey.Primary
                        : DatabaseKey.Reporting;
                    DatabaseDefinition<DatabaseKey> definition = router.Resolve(key);
                    Assert.Equal(key, definition.Key);
                }
                catch (Exception exception)
                {
                    failures.Enqueue(exception);
                }
            });

        Assert.Empty(failures);
    }

    [Fact]
    public void Concurrent_equal_selection_is_safe()
    {
        var selection = new DatabaseSelection<DatabaseKey>(TestDefinitions.CreateRouter());

        Parallel.For(0, 2_000, _ => selection.Select(DatabaseKey.Primary));

        Assert.Equal(DatabaseKey.Primary, selection.SelectedKey);
    }
}
