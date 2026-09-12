using DbRouter.DependencyInjection.Builders;
using DbRouter.PostgreSql.Providers;

namespace DbRouter.PostgreSql.Extensions;

/// <summary>Adds PostgreSQL support to a DbRouter builder.</summary>
public static class PostgreSqlDbRouterBuilderExtensions
{
    /// <summary>Registers the PostgreSQL connection provider.</summary>
    public static DbRouterBuilder<TKey> AddPostgreSql<TKey>(
        this DbRouterBuilder<TKey> builder)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddProvider<PostgreSqlDbConnectionProvider>();
    }

    /// <summary>Registers the provider and one inline PostgreSQL definition.</summary>
    public static DbRouterBuilder<TKey> AddPostgreSql<TKey>(
        this DbRouterBuilder<TKey> builder,
        TKey key,
        string connectionString)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder
            .AddPostgreSql()
            .AddDatabase(key, PostgreSqlDbConnectionProvider.Id, connectionString);
    }
}
