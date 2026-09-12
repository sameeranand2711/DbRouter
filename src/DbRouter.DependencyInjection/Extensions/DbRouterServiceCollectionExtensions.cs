using DbRouter.Core.Abstractions.Connections;
using DbRouter.Core.Abstractions.Resolvers;
using DbRouter.Core.Abstractions.Scoping;
using DbRouter.Core.Resolution;
using DbRouter.DependencyInjection.Builders;
using DbRouter.DependencyInjection.Factories;
using DbRouter.DependencyInjection.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DbRouter.DependencyInjection.Extensions;

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

        services.TryAddSingleton<
            IDbRouter<TKey>,
            global::DbRouter.Core.Resolution.DbRouter<TKey>>();
        services.TryAddSingleton<DbConnectionProviderRegistry>();
        services.TryAddScoped<IDatabaseSelection<TKey>, DatabaseSelection<TKey>>();
        services.TryAddScoped<IDbConnectionFactory<TKey>, DbConnectionFactory<TKey>>();

        return services;
    }
}
