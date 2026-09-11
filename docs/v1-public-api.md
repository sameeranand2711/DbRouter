# DbRouter V1 Public API

This document fixes the intended V1 surface. Minor implementation-only constructors and attributes may be internal, but public behavior and names must remain consistent with this contract.

## DbRouter.Core

```csharp
namespace DbRouter;

public sealed class DatabaseDefinition<TKey> where TKey : notnull
{
    public DatabaseDefinition(TKey key, string providerId, string connectionString);

    public TKey Key { get; }
    public string ProviderId { get; }
    public string ConnectionString { get; }
    public override string ToString(); // always redacts ConnectionString
}

public interface IDatabaseDefinitionProvider<TKey> where TKey : notnull
{
    IReadOnlyCollection<DatabaseDefinition<TKey>> GetDefinitions();
}

public sealed class StaticDatabaseDefinitionProvider<TKey> :
    IDatabaseDefinitionProvider<TKey> where TKey : notnull
{
    public StaticDatabaseDefinitionProvider(
        IEnumerable<DatabaseDefinition<TKey>> definitions);

    public IReadOnlyCollection<DatabaseDefinition<TKey>> GetDefinitions();
}

public interface IDbRouter<TKey> where TKey : notnull
{
    DatabaseDefinition<TKey> Resolve(TKey key);

    bool TryResolve(
        TKey key,
        [NotNullWhen(true)] out DatabaseDefinition<TKey>? definition);
}

public sealed class DbRouter<TKey> : IDbRouter<TKey> where TKey : notnull
{
    public DbRouter(IDatabaseDefinitionProvider<TKey> definitionProvider);

    public DatabaseDefinition<TKey> Resolve(TKey key);

    public bool TryResolve(
        TKey key,
        [NotNullWhen(true)] out DatabaseDefinition<TKey>? definition);
}

public interface IDatabaseSelection<TKey> where TKey : notnull
{
    bool HasSelection { get; }
    TKey SelectedKey { get; }
    void Select(TKey key);
    bool TryGetSelected([MaybeNullWhen(false)] out TKey key);
}

public sealed class DatabaseSelection<TKey> : IDatabaseSelection<TKey>
    where TKey : notnull
{
    public DatabaseSelection(IDbRouter<TKey> router);

    public bool HasSelection { get; }
    public TKey SelectedKey { get; }
    public void Select(TKey key);
    public bool TryGetSelected([MaybeNullWhen(false)] out TKey key);
}

public interface IDbConnectionProvider
{
    string ProviderId { get; }
    DbConnection Create(string connectionString);
}

public interface IDbConnectionFactory<TKey> where TKey : notnull
{
    DbConnection Create(TKey key); // explicit
    DbConnection Create();         // current scoped selection
}
```

Public exception types, each in its own file:

- `DatabaseDefinitionValidationException`
- `DatabaseNotFoundException`
- `DatabaseSelectionMissingException`
- `DatabaseSelectionConflictException`
- `DbConnectionProviderNotFoundException`
- `DbConnectionProviderRegistrationException`
- `DbConnectionCreationException`

Exceptions do not expose connection strings as properties or message content. `DatabaseNotFoundException` intentionally does not stringify arbitrary keys into its message.

## DbRouter.DependencyInjection

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class DbRouterServiceCollectionExtensions
{
    public static IServiceCollection AddDbRouter<TKey>(
        this IServiceCollection services,
        Action<DbRouterBuilder<TKey>> configure)
        where TKey : notnull;
}
```

```csharp
namespace DbRouter.DependencyInjection;

public sealed class DbRouterBuilder<TKey> where TKey : notnull
{
    public IServiceCollection Services { get; }

    public DbRouterBuilder<TKey> AddDatabase(
        DatabaseDefinition<TKey> definition);

    public DbRouterBuilder<TKey> AddDatabase(
        TKey key,
        string providerId,
        string connectionString);

    public DbRouterBuilder<TKey> UseDefinitionProvider<TProvider>()
        where TProvider : class, IDatabaseDefinitionProvider<TKey>;

    public DbRouterBuilder<TKey> AddProvider<TProvider>()
        where TProvider : class, IDbConnectionProvider;

    public DbRouterBuilder<TKey> AddProvider(
        IDbConnectionProvider provider);
}
```

The builder is a composition API, not a runtime service locator. Inline definitions cannot be mixed with `UseDefinitionProvider`. Provider implementations are registered as singletons and must be safe for concurrent `Create` calls.

## Provider packages

```csharp
namespace DbRouter.SqlServer;

public sealed class SqlServerDbConnectionProvider : IDbConnectionProvider
{
    public const string Id = "sqlserver";
    public string ProviderId { get; }
    public DbConnection Create(string connectionString);
}

public static class SqlServerDbRouterBuilderExtensions
{
    public static DbRouterBuilder<TKey> AddSqlServer<TKey>(
        this DbRouterBuilder<TKey> builder) where TKey : notnull;

    public static DbRouterBuilder<TKey> AddSqlServer<TKey>(
        this DbRouterBuilder<TKey> builder,
        TKey key,
        string connectionString) where TKey : notnull;
}
```

`DbRouter.PostgreSql` exposes the corresponding `PostgreSqlDbConnectionProvider` with ID `postgresql` and `AddPostgreSql` overloads. Provider `Create` methods return a non-null, closed connection and do not open it.

## DbRouter.EntityFrameworkCore

```csharp
namespace DbRouter.EntityFrameworkCore;

public interface IDbContextResolver<TKey, TContext>
    where TKey : notnull
    where TContext : DbContext
{
    TContext Create(TKey key);
}

public sealed class DbRouterEntityFrameworkBuilder<TContext>
    where TContext : DbContext
{
    public DbRouterEntityFrameworkBuilder<TContext> AddProvider(
        string providerId,
        Action<DbContextOptionsBuilder<TContext>, string> configure);
}
```

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class DbRouterEntityFrameworkServiceCollectionExtensions
{
    public static IServiceCollection AddDbRouterEntityFrameworkCore<TKey, TContext>(
        this IServiceCollection services,
        Func<DbContextOptions<TContext>, TContext> contextFactory,
        Action<DbRouterEntityFrameworkBuilder<TContext>> configure)
        where TKey : notnull
        where TContext : DbContext;
}
```

The registration adds `IDbContextResolver<TKey,TContext>` and scoped `TContext`. Provider-specific packages can add fluent extensions over `DbRouterEntityFrameworkBuilder<TContext>` without changes to the central EF integration.

The EF package also exposes three safe exception types so applications can distinguish missing provider configuration, duplicate registration, and sanitized context-construction failure:

- `DbContextProviderNotFoundException`
- `DbContextProviderRegistrationException`
- `DbContextCreationException`

## Typical composition

```csharp
services.AddDbRouter<DatabaseKey>(builder =>
{
    builder.AddSqlServer(DatabaseKey.Primary, primaryConnectionString);
    builder.AddPostgreSql(DatabaseKey.Reporting, reportingConnectionString);
});

services.AddDbRouterEntityFrameworkCore<DatabaseKey, ApplicationDbContext>(
    options => new ApplicationDbContext(options),
    builder =>
    {
        builder.AddProvider(
            SqlServerDbConnectionProvider.Id,
            (options, connectionString) => options.UseSqlServer(connectionString));
        builder.AddProvider(
            PostgreSqlDbConnectionProvider.Id,
            (options, connectionString) => options.UseNpgsql(connectionString));
    });
```

Application repositories inject `ApplicationDbContext`; explicit multi-database operations inject `IDbContextResolver<DatabaseKey,ApplicationDbContext>`.
