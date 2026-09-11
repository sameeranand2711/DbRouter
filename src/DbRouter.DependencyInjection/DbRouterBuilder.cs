using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DbRouter.DependencyInjection;

/// <summary>Configures one strongly typed DbRouter registration.</summary>
public sealed class DbRouterBuilder<TKey>
    where TKey : notnull
{
    private readonly List<DatabaseDefinition<TKey>> _definitions = [];
    private bool _usesCustomDefinitionProvider;

    internal DbRouterBuilder(IServiceCollection services)
    {
        _services = services;
    }

    private readonly IServiceCollection _services;

    /// <summary>Adds one inline static database definition.</summary>
    public DbRouterBuilder<TKey> AddDatabase(DatabaseDefinition<TKey> definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        EnsureInlineDefinitionsAllowed();
        _definitions.Add(definition);
        return this;
    }

    /// <summary>Adds one inline static database definition.</summary>
    public DbRouterBuilder<TKey> AddDatabase(
        TKey key,
        string providerId,
        string connectionString) =>
        AddDatabase(new DatabaseDefinition<TKey>(key, providerId, connectionString));

    /// <summary>Uses a custom static V1 definition provider instead of inline definitions.</summary>
    public DbRouterBuilder<TKey> UseDefinitionProvider<TProvider>()
        where TProvider : class, IDatabaseDefinitionProvider<TKey>
    {
        if (_usesCustomDefinitionProvider)
        {
            throw new InvalidOperationException(
                "Only one database definition provider can be configured for a router.");
        }

        if (_definitions.Count > 0)
        {
            throw new InvalidOperationException(
                "A custom database definition provider cannot be combined with inline definitions.");
        }

        _usesCustomDefinitionProvider = true;
        _services.AddSingleton<IDatabaseDefinitionProvider<TKey>, TProvider>();
        return this;
    }

    /// <summary>Adds a stateless connection provider type if that implementation is not already registered.</summary>
    public DbRouterBuilder<TKey> AddProvider<TProvider>()
        where TProvider : class, IDbConnectionProvider
    {
        _services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IDbConnectionProvider, TProvider>());
        return this;
    }

    /// <summary>Adds a connection provider instance.</summary>
    public DbRouterBuilder<TKey> AddProvider(IDbConnectionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _services.AddSingleton(provider);
        return this;
    }

    internal void Complete()
    {
        if (!_usesCustomDefinitionProvider)
        {
            var provider = new StaticDatabaseDefinitionProvider<TKey>(_definitions);
            _services.AddSingleton<IDatabaseDefinitionProvider<TKey>>(provider);
        }
    }

    private void EnsureInlineDefinitionsAllowed()
    {
        if (_usesCustomDefinitionProvider)
        {
            throw new InvalidOperationException(
                "Inline definitions cannot be combined with a custom database definition provider.");
        }
    }
}
