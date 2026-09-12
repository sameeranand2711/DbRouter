namespace DbRouter.EntityFrameworkCore.Tests;

internal sealed class TestRepository
{
    public TestRepository(TestDbContext context)
    {
        Context = context;
    }

    public TestDbContext Context { get; }

    public void Add(TestEntity entity) => Context.Entities.Add(entity);
}
