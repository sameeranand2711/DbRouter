using Microsoft.EntityFrameworkCore;

namespace DbRouter.EntityFrameworkCore;

internal sealed class DbContextResolver<TKey, TContext> : IDbContextResolver<TKey, TContext>
    where TKey : notnull
    where TContext : DbContext
{
    private readonly IDbRouter<TKey> _router;
    private readonly DbContextOptionsConfiguratorRegistry<TContext> _configurators;
    private readonly DbContextActivator<TContext> _activator;

    public DbContextResolver(
        IDbRouter<TKey> router,
        DbContextOptionsConfiguratorRegistry<TContext> configurators,
        DbContextActivator<TContext> activator)
    {
        _router = router;
        _configurators = configurators;
        _activator = activator;
    }

    public TContext Create(TKey key)
    {
        DatabaseDefinition<TKey> definition = _router.Resolve(key);
        Action<DbContextOptionsBuilder<TContext>, string> configure =
            _configurators.Resolve(definition.ProviderId);
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();

        try
        {
            configure(optionsBuilder, definition.ConnectionString);
            TContext context = _activator.Activate(optionsBuilder.Options);

            return context ?? throw new DbContextCreationException();
        }
        catch (DbContextCreationException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new DbContextCreationException();
        }
    }
}
