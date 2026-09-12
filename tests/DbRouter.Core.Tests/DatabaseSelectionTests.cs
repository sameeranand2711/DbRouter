namespace DbRouter.Core.Tests;

using Microsoft.Extensions.DependencyInjection;

public sealed class DatabaseSelectionTests
{
    [Fact]
    public void Selection_starts_empty_and_missing_selection_is_predictable()
    {
        var selection = new DatabaseSelection<DatabaseKey>(TestDefinitions.CreateRouter());

        Assert.False(selection.HasSelection);
        Assert.False(selection.TryGetSelected(out _));
        Assert.Throws<DatabaseSelectionMissingException>(() => selection.SelectedKey);
    }

    [Fact]
    public void Valid_selection_can_be_read()
    {
        var selection = new DatabaseSelection<DatabaseKey>(TestDefinitions.CreateRouter());

        selection.Select(DatabaseKey.Reporting);

        Assert.True(selection.HasSelection);
        Assert.Equal(DatabaseKey.Reporting, selection.SelectedKey);
        Assert.True(selection.TryGetSelected(out DatabaseKey key));
        Assert.Equal(DatabaseKey.Reporting, key);
    }

    [Fact]
    public void Repeating_same_selection_is_idempotent()
    {
        var selection = new DatabaseSelection<DatabaseKey>(TestDefinitions.CreateRouter());

        selection.Select(DatabaseKey.Primary);
        selection.Select(DatabaseKey.Primary);

        Assert.Equal(DatabaseKey.Primary, selection.SelectedKey);
    }

    [Fact]
    public void Changing_selection_is_rejected()
    {
        var selection = new DatabaseSelection<DatabaseKey>(TestDefinitions.CreateRouter());
        selection.Select(DatabaseKey.Primary);

        Assert.Throws<DatabaseSelectionConflictException>(
            () => selection.Select(DatabaseKey.Reporting));
        Assert.Equal(DatabaseKey.Primary, selection.SelectedKey);
    }

    [Fact]
    public void Unknown_selection_is_rejected_without_mutating_state()
    {
        var selection = new DatabaseSelection<DatabaseKey>(TestDefinitions.CreateRouter());

        Assert.Throws<DatabaseNotFoundException>(() => selection.Select((DatabaseKey)999));
        Assert.False(selection.HasSelection);
    }

    [Fact]
    public void Separate_selection_instances_are_isolated()
    {
        DbRouter<DatabaseKey> router = TestDefinitions.CreateRouter();
        var firstScope = new DatabaseSelection<DatabaseKey>(router);
        var secondScope = new DatabaseSelection<DatabaseKey>(router);

        firstScope.Select(DatabaseKey.Primary);
        secondScope.Select(DatabaseKey.Reporting);

        Assert.Equal(DatabaseKey.Primary, firstScope.SelectedKey);
        Assert.Equal(DatabaseKey.Reporting, secondScope.SelectedKey);
    }

    [Fact]
    public void Separate_dependency_injection_scopes_are_isolated()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDbRouter<DatabaseKey>>(TestDefinitions.CreateRouter());
        services.AddScoped<IDatabaseSelection<DatabaseKey>, DatabaseSelection<DatabaseKey>>();
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope firstScope = provider.CreateScope();
        using IServiceScope secondScope = provider.CreateScope();

        IDatabaseSelection<DatabaseKey> first =
            firstScope.ServiceProvider.GetRequiredService<IDatabaseSelection<DatabaseKey>>();
        IDatabaseSelection<DatabaseKey> second =
            secondScope.ServiceProvider.GetRequiredService<IDatabaseSelection<DatabaseKey>>();

        first.Select(DatabaseKey.Primary);
        second.Select(DatabaseKey.Reporting);

        Assert.NotSame(first, second);
        Assert.Equal(DatabaseKey.Primary, first.SelectedKey);
        Assert.Equal(DatabaseKey.Reporting, second.SelectedKey);
    }

    [Fact]
    public void Explicit_resolution_does_not_change_scoped_selection()
    {
        DbRouter<DatabaseKey> router = TestDefinitions.CreateRouter();
        var selection = new DatabaseSelection<DatabaseKey>(router);
        selection.Select(DatabaseKey.Primary);

        DatabaseDefinition<DatabaseKey> explicitDefinition = router.Resolve(DatabaseKey.Reporting);

        Assert.Equal(DatabaseKey.Reporting, explicitDefinition.Key);
        Assert.Equal(DatabaseKey.Primary, selection.SelectedKey);
    }
}
