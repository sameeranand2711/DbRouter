using Microsoft.EntityFrameworkCore;

namespace DbRouter.EntityFrameworkCore;

internal sealed class DbContextActivator<TContext>
    where TContext : DbContext
{
    public DbContextActivator(Func<DbContextOptions<TContext>, TContext> activate)
    {
        Activate = activate;
    }

    public Func<DbContextOptions<TContext>, TContext> Activate { get; }
}
