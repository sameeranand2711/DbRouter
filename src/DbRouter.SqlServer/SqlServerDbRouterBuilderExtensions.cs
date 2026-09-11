using DbRouter.DependencyInjection;

namespace DbRouter.SqlServer;

/// <summary>Adds Microsoft SQL Server support to a DbRouter builder.</summary>
public static class SqlServerDbRouterBuilderExtensions
{
    /// <summary>Registers the SQL Server connection provider.</summary>
    public static DbRouterBuilder<TKey> AddSqlServer<TKey>(
        this DbRouterBuilder<TKey> builder)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddProvider<SqlServerDbConnectionProvider>();
    }

    /// <summary>Registers the provider and one inline SQL Server definition.</summary>
    public static DbRouterBuilder<TKey> AddSqlServer<TKey>(
        this DbRouterBuilder<TKey> builder,
        TKey key,
        string connectionString)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder
            .AddSqlServer()
            .AddDatabase(key, SqlServerDbConnectionProvider.Id, connectionString);
    }
}
