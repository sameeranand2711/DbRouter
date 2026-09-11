using DbRouter.Core.Abstractions.Scoping;
using DbRouter.EntityFrameworkCore.Abstractions.Resolvers;
using DbRouter.EntityFrameworkCore.Factories;
using DbRouter.EntityFrameworkCore.Options;
using DbRouter.EntityFrameworkCore.Resolution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DbRouter.EntityFrameworkCore.Extensions;

/// <summary>Registers optional provider-neutral EF Core integration for DbRouter.</summary>
public static class DbRouterEntityFrameworkServiceCollectionExtensions
{
    /// <summary>
    /// Registers explicit context resolution and one conventionally injected scoped context.
    /// </summary>
    public static IServiceCollection AddDbRouterEntityFrameworkCore<TKey, TContext>(
        this IServiceCollection services,
        Func<Microsoft.EntityFrameworkCore.DbContextOptions<TContext>, TContext> contextFactory,
        Action<DbRouterEntityFrameworkBuilder<TContext>> configure)
        where TKey : notnull
        where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(contextFactory);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new DbRouterEntityFrameworkBuilder<TContext>();
        configure(builder);

        services.AddSingleton(builder.Build());
        services.AddSingleton(new DbContextActivator<TContext>(contextFactory));
        services.TryAddScoped<
            IDbContextResolver<TKey, TContext>,
            DbContextResolver<TKey, TContext>>();
        services.TryAddScoped<TContext>(serviceProvider =>
        {
            IDatabaseSelection<TKey> selection = serviceProvider
                .GetRequiredService<IDatabaseSelection<TKey>>();
            IDbContextResolver<TKey, TContext> resolver = serviceProvider
                .GetRequiredService<IDbContextResolver<TKey, TContext>>();
            return resolver.Create(selection.SelectedKey);
        });

        return services;
    }
}
