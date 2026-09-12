namespace DbRouter.Core.Tests;

public sealed class DbRouterTests
{
    [Fact]
    public void Resolve_returns_known_definition()
    {
        DbRouter<DatabaseKey> router = TestDefinitions.CreateRouter();

        DatabaseDefinition<DatabaseKey> definition = router.Resolve(DatabaseKey.Primary);

        Assert.Equal(DatabaseKey.Primary, definition.Key);
        Assert.Equal("sqlserver", definition.ProviderId);
    }

    [Fact]
    public void Resolve_throws_predictably_for_unknown_key()
    {
        var router = new DbRouter<string>(
            new StaticDatabaseDefinitionProvider<string>(
            [
                new("known", "sqlserver", TestDefinitions.PrimaryConnectionString),
            ]));

        DatabaseNotFoundException exception = Assert.Throws<DatabaseNotFoundException>(
            () => router.Resolve("unknown"));

        Assert.DoesNotContain("unknown", exception.Message);
        Assert.DoesNotContain("primary-secret", exception.ToString());
    }

    [Fact]
    public void TryResolve_returns_false_for_unknown_key()
    {
        DbRouter<DatabaseKey> router = TestDefinitions.CreateRouter();

        bool found = router.TryResolve((DatabaseKey)999, out DatabaseDefinition<DatabaseKey>? definition);

        Assert.False(found);
        Assert.Null(definition);
    }

    [Fact]
    public void Multiple_keys_resolve_independently()
    {
        DbRouter<DatabaseKey> router = TestDefinitions.CreateRouter();

        DatabaseDefinition<DatabaseKey> primary = router.Resolve(DatabaseKey.Primary);
        DatabaseDefinition<DatabaseKey> reporting = router.Resolve(DatabaseKey.Reporting);

        Assert.NotSame(primary, reporting);
        Assert.Equal("sqlserver", primary.ProviderId);
        Assert.Equal("postgresql", reporting.ProviderId);
    }

    [Fact]
    public void Strongly_typed_value_object_keys_are_supported()
    {
        var key = new DatabaseId(Guid.NewGuid());
        var router = new DbRouter<DatabaseId>(
            new StaticDatabaseDefinitionProvider<DatabaseId>(
            [
                new(key, "custom", "opaque-secret"),
            ]));

        DatabaseDefinition<DatabaseId> definition = router.Resolve(key);

        Assert.Equal(key, definition.Key);
    }

    [Fact]
    public void Null_reference_key_is_rejected()
    {
        var router = new DbRouter<string>(
            new StaticDatabaseDefinitionProvider<string>(
            [
                new("known", "custom", "opaque-secret"),
            ]));

        Assert.Throws<ArgumentNullException>(() => router.Resolve(null!));
        Assert.Throws<ArgumentNullException>(() => router.TryResolve(null!, out _));
    }
}
