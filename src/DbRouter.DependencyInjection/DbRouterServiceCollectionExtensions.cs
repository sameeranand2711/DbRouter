using DbRouter.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers DbRouter with Microsoft dependency injection.</summary>
public static class DbRouterServiceCollectionExtensions
{
    /// <summary>Registers one statically configured router and its scoped connection services.</summary>
    public static IServiceCollection AddDbRouter<TKey>(
        this IServiceCollection services,
        Action<DbRouterBuilder<TKey>> configure)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new DbRouterBuilder<TKey>(services);
        configure(builder);
        builder.Complete();

        services.TryAddSingleton<global::DbRouter.IDbRouter<TKey>, global::DbRouter.DbRouter<TKey>>();
        services.TryAddSingleton<DbConnectionProviderRegistry>();
        services.TryAddScoped<
            global::DbRouter.IDatabaseSelection<TKey>,
            global::DbRouter.DatabaseSelection<TKey>>();
        services.TryAddScoped<
            global::DbRouter.IDbConnectionFactory<TKey>,
            DbConnectionFactory<TKey>>();

        return services;
    }
}
