using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbRouter.EntityFrameworkCore.Tests;

public sealed class ScopedDbContextTests
{
    [Fact]
    public void Same_scope_reuses_context_for_conventional_repository_injection()
    {
        using ServiceProvider provider = TestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<IDatabaseSelection<DatabaseKey>>()
            .Select(DatabaseKey.Primary);

        TestDbContext context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        TestDbContext sameContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        TestRepository repository = scope.ServiceProvider.GetRequiredService<TestRepository>();

        Assert.Same(context, sameContext);
        Assert.Same(context, repository.Context);
    }

    [Fact]
    public async Task Different_scopes_get_distinct_contexts_and_database_configuration()
    {
        using ServiceProvider provider = TestServices.Build();
        using IServiceScope primaryScope = provider.CreateScope();
        using IServiceScope reportingScope = provider.CreateScope();
        primaryScope.ServiceProvider
            .GetRequiredService<IDatabaseSelection<DatabaseKey>>()
            .Select(DatabaseKey.Primary);
        reportingScope.ServiceProvider
            .GetRequiredService<IDatabaseSelection<DatabaseKey>>()
            .Select(DatabaseKey.Reporting);
        TestDbContext primary = primaryScope.ServiceProvider.GetRequiredService<TestDbContext>();
        TestDbContext reporting = reportingScope.ServiceProvider.GetRequiredService<TestDbContext>();

        primary.Entities.Add(new TestEntity { Id = 1, Value = "primary-only" });
        await primary.SaveChangesAsync();

        Assert.NotSame(primary, reporting);
        Assert.Single(await primary.Entities.ToListAsync());
        Assert.Empty(await reporting.Entities.ToListAsync());
    }

    [Fact]
    public void Scope_owns_and_disposes_scoped_context()
    {
        using ServiceProvider provider = TestServices.Build();
        IServiceScope scope = provider.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<IDatabaseSelection<DatabaseKey>>()
            .Select(DatabaseKey.Primary);
        TestDbContext context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        scope.Dispose();

        Assert.True(context.IsDisposed);
    }

    [Fact]
    public void Scoped_context_requires_a_selection()
    {
        using ServiceProvider provider = TestServices.Build();
        using IServiceScope scope = provider.CreateScope();

        Assert.Throws<DatabaseSelectionMissingException>(
            () => scope.ServiceProvider.GetRequiredService<TestDbContext>());
    }

    [Fact]
    public async Task Repositories_share_normal_unit_of_work_state()
    {
        using ServiceProvider provider = TestServices.Build();
        using IServiceScope scope = provider.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<IDatabaseSelection<DatabaseKey>>()
            .Select(DatabaseKey.Primary);
        TestRepository firstRepository = scope.ServiceProvider.GetRequiredService<TestRepository>();
        TestRepository secondRepository = scope.ServiceProvider.GetRequiredService<TestRepository>();

        firstRepository.Add(new TestEntity { Id = 1, Value = "tracked" });

        Assert.Same(firstRepository.Context, secondRepository.Context);
        Assert.Single(secondRepository.Context.ChangeTracker.Entries<TestEntity>());
        await secondRepository.Context.SaveChangesAsync();
        Assert.Single(await firstRepository.Context.Entities.ToListAsync());
    }
}
