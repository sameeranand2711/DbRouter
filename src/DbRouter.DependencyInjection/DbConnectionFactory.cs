using System.Data;
using System.Data.Common;

namespace DbRouter.DependencyInjection;

internal sealed class DbConnectionFactory<TKey> : IDbConnectionFactory<TKey>
    where TKey : notnull
{
    private readonly IDbRouter<TKey> _router;
    private readonly IDatabaseSelection<TKey> _selection;
    private readonly DbConnectionProviderRegistry _providers;

    public DbConnectionFactory(
        IDbRouter<TKey> router,
        IDatabaseSelection<TKey> selection,
        DbConnectionProviderRegistry providers)
    {
        _router = router;
        _selection = selection;
        _providers = providers;
    }

    public DbConnection Create(TKey key) => Create(_router.Resolve(key));

    public DbConnection Create() => Create(_router.Resolve(_selection.SelectedKey));

    private DbConnection Create(DatabaseDefinition<TKey> definition)
    {
        IDbConnectionProvider provider = _providers.Resolve(definition.ProviderId);
        DbConnection? connection;

        try
        {
            connection = provider.Create(definition.ConnectionString);
        }
        catch (Exception)
        {
            throw new DbConnectionCreationException();
        }

        if (connection is null)
        {
            throw new DbConnectionCreationException();
        }

        if (connection.State == ConnectionState.Closed)
        {
            return connection;
        }

        try
        {
            connection.Dispose();
        }
        catch (Exception)
        {
            // The connection is rejected regardless; never expose provider diagnostics here.
        }

        throw new DbConnectionCreationException();
    }
}
