using Microsoft.EntityFrameworkCore;

namespace DbRouter.EntityFrameworkCore.Tests;

internal sealed class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public bool IsDisposed { get; private set; }

    public DbSet<TestEntity> Entities => Set<TestEntity>();

    public override void Dispose()
    {
        IsDisposed = true;
        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        IsDisposed = true;
        await base.DisposeAsync();
    }
}
