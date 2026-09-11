using DbRouter.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbRouter.EntityFrameworkCore.Tests;

public sealed class ExplicitDbContextTests
{
    [Fact]
    public async Task Explicit_key_creates_requested_independently_configured_context()
    {
        using ServiceProvider provider = TestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        IDbContextResolver<DatabaseKey, TestDbContext> resolver = scope.ServiceProvider
            .GetRequiredService<IDbContextResolver<DatabaseKey, TestDbContext>>();
        await using TestDbContext primary = resolver.Create(DatabaseKey.Primary);
        await using TestDbContext reporting = resolver.Create(DatabaseKey.Reporting);

        primary.Entities.Add(new TestEntity { Id = 1, Value = "primary-only" });
        await primary.SaveChangesAsync();

        Assert.Single(await primary.Entities.ToListAsync());
        Assert.Empty(await reporting.Entities.ToListAsync());
        Assert.NotSame(primary, reporting);
    }

    [Fact]
    public async Task Repeated_explicit_creation_returns_distinct_contexts_for_same_key()
    {
        using ServiceProvider provider = TestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        IDbContextResolver<DatabaseKey, TestDbContext> resolver = scope.ServiceProvider
            .GetRequiredService<IDbContextResolver<DatabaseKey, TestDbContext>>();

        await using TestDbContext first = resolver.Create(DatabaseKey.Primary);
        await using TestDbContext second = resolver.Create(DatabaseKey.Primary);

        Assert.NotSame(first, second);
    }

    [Fact]
    public async Task Caller_disposes_explicit_context()
    {
        using ServiceProvider provider = TestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        IDbContextResolver<DatabaseKey, TestDbContext> resolver = scope.ServiceProvider
            .GetRequiredService<IDbContextResolver<DatabaseKey, TestDbContext>>();
        TestDbContext context = resolver.Create(DatabaseKey.Primary);

        await context.DisposeAsync();

        Assert.True(context.IsDisposed);
    }

    [Fact]
    public async Task Explicit_creation_does_not_corrupt_scoped_selection()
    {
        using ServiceProvider provider = TestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        IDatabaseSelection<DatabaseKey> selection = scope.ServiceProvider
            .GetRequiredService<IDatabaseSelection<DatabaseKey>>();
        selection.Select(DatabaseKey.Primary);
        IDbContextResolver<DatabaseKey, TestDbContext> resolver = scope.ServiceProvider
            .GetRequiredService<IDbContextResolver<DatabaseKey, TestDbContext>>();
        await using TestDbContext explicitReporting = resolver.Create(DatabaseKey.Reporting);
        explicitReporting.Entities.Add(new TestEntity { Id = 1, Value = "reporting-only" });
        await explicitReporting.SaveChangesAsync();

        TestDbContext scopedPrimary = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        Assert.Empty(await scopedPrimary.Entities.ToListAsync());
        Assert.Equal(DatabaseKey.Primary, selection.SelectedKey);
    }

    [Fact]
    public void Unknown_explicit_key_fails_predictably()
    {
        using ServiceProvider provider = TestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        IDbContextResolver<DatabaseKey, TestDbContext> resolver = scope.ServiceProvider
            .GetRequiredService<IDbContextResolver<DatabaseKey, TestDbContext>>();

        Assert.Throws<DatabaseNotFoundException>(() => resolver.Create((DatabaseKey)999));
    }
}
