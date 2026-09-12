# DbRouter V1 Public API

This document records the intended V1 public surface after responsibility-based namespace alignment. Namespace names are part of the contract.

## DbRouter.Core

### Models

```csharp
namespace DbRouter.Core.Models;

public sealed class DatabaseDefinition<TKey> where TKey : notnull
{
    public DatabaseDefinition(TKey key, string providerId, string connectionString);
    public TKey Key { get; }
    public string ProviderId { get; }
    public string ConnectionString { get; }
    public override string ToString();
}
```

### Provider contracts and implementation

```csharp
namespace DbRouter.Core.Abstractions.Providers;

public interface IDatabaseDefinitionProvider<TKey> where TKey : notnull
{
    IReadOnlyCollection<DatabaseDefinition<TKey>> GetDefinitions();
}

public interface IDbConnectionProvider
{
    string ProviderId { get; }
    DbConnection Create(string connectionString);
}
```

```csharp
namespace DbRouter.Core.Providers;

public sealed class StaticDatabaseDefinitionProvider<TKey> :
    IDatabaseDefinitionProvider<TKey> where TKey : notnull
{
    public StaticDatabaseDefinitionProvider(
        IEnumerable<DatabaseDefinition<TKey>> definitions);
    public IReadOnlyCollection<DatabaseDefinition<TKey>> GetDefinitions();
}
```

### Resolution, connection, and scoping contracts

```csharp
namespace DbRouter.Core.Abstractions.Resolvers;

public interface IDbRouter<TKey> where TKey : notnull
{
    DatabaseDefinition<TKey> Resolve(TKey key);
    bool TryResolve(
        TKey key,
        [NotNullWhen(true)] out DatabaseDefinition<TKey>? definition);
}
```

```csharp
namespace DbRouter.Core.Abstractions.Connections;

public interface IDbConnectionFactory<TKey> where TKey : notnull
{
    DbConnection Create(TKey key);
    DbConnection Create();
}
```

```csharp
namespace DbRouter.Core.Abstractions.Scoping;

public interface IDatabaseSelection<TKey> where TKey : notnull
{
    bool HasSelection { get; }
    TKey SelectedKey { get; }
    void Select(TKey key);
    bool TryGetSelected([MaybeNullWhen(false)] out TKey key);
}
```

```csharp
namespace DbRouter.Core.Resolution;

public sealed class DbRouter<TKey> : IDbRouter<TKey> where TKey : notnull
{
    // Implements the resolver contract.
}

public sealed class DatabaseSelection<TKey> : IDatabaseSelection<TKey>
    where TKey : notnull
{
    // Implements the scoped-selection contract.
}
```

The concrete classes expose the members of their corresponding interfaces and their dependency-injection constructors.

Namespace `DbRouter.Core.Exceptions` contains:

- `DatabaseDefinitionValidationException`
- `DatabaseNotFoundException`
- `DatabaseSelectionMissingException`
- `DatabaseSelectionConflictException`
- `DbConnectionProviderNotFoundException`
- `DbConnectionProviderRegistrationException`
- `DbConnectionCreationException`

Exceptions do not expose connection strings as properties or message content. `DatabaseNotFoundException` intentionally does not stringify arbitrary keys.

## DbRouter.DependencyInjection

```csharp
namespace DbRouter.DependencyInjection.Extensions;

public static class DbRouterServiceCollectionExtensions
{
    public static IServiceCollection AddDbRouter<TKey>(
        this IServiceCollection services,
        Action<DbRouterBuilder<TKey>> configure)
        where TKey : notnull;
}
```

```csharp
namespace DbRouter.DependencyInjection.Builders;

public sealed class DbRouterBuilder<TKey> where TKey : notnull
{
    public DbRouterBuilder<TKey> AddDatabase(DatabaseDefinition<TKey> definition);
    public DbRouterBuilder<TKey> AddDatabase(
        TKey key,
        string providerId,
        string connectionString);
    public DbRouterBuilder<TKey> UseDefinitionProvider<TProvider>()
        where TProvider : class, IDatabaseDefinitionProvider<TKey>;
    public DbRouterBuilder<TKey> AddProvider<TProvider>()
        where TProvider : class, IDbConnectionProvider;
    public DbRouterBuilder<TKey> AddProvider(IDbConnectionProvider provider);
}
```

The builder is a composition API, not a runtime service locator. Inline definitions cannot be mixed with `UseDefinitionProvider`.

## Provider packages

`DbRouter.SqlServer.Providers` contains `SqlServerDbConnectionProvider`; `DbRouter.SqlServer.Extensions` contains `SqlServerDbRouterBuilderExtensions`.

`DbRouter.PostgreSql.Providers` contains `PostgreSqlDbConnectionProvider`; `DbRouter.PostgreSql.Extensions` contains `PostgreSqlDbRouterBuilderExtensions`.

Each provider class implements `IDbConnectionProvider`, exposes its stable `Id`, and returns a new closed connection. Each extension class has overloads both to register the provider alone and to add one inline database definition.

## DbRouter.EntityFrameworkCore

```csharp
namespace DbRouter.EntityFrameworkCore.Abstractions.Resolvers;

public interface IDbContextResolver<TKey, TContext>
    where TKey : notnull
    where TContext : DbContext
{
    TContext Create(TKey key);
}
```

```csharp
namespace DbRouter.EntityFrameworkCore.Options;

public sealed class DbRouterEntityFrameworkBuilder<TContext>
    where TContext : DbContext
{
    public DbRouterEntityFrameworkBuilder<TContext> AddProvider(
        string providerId,
        Action<DbContextOptionsBuilder<TContext>, string> configure);
}
```

```csharp
namespace DbRouter.EntityFrameworkCore.Extensions;

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

Namespace `DbRouter.EntityFrameworkCore.Exceptions` contains `DbContextProviderNotFoundException`, `DbContextProviderRegistrationException`, and `DbContextCreationException`.

## Typical imports

```csharp
using DbRouter.Core.Abstractions.Connections;
using DbRouter.Core.Abstractions.Resolvers;
using DbRouter.Core.Abstractions.Scoping;
using DbRouter.DependencyInjection.Extensions;
using DbRouter.EntityFrameworkCore.Abstractions.Resolvers;
using DbRouter.EntityFrameworkCore.Extensions;
using DbRouter.PostgreSql.Extensions;
using DbRouter.PostgreSql.Providers;
using DbRouter.SqlServer.Extensions;
using DbRouter.SqlServer.Providers;
```

Applications import only the responsibility namespaces used by their composition and runtime code.
