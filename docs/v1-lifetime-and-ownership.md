# DbRouter V1 Lifetime and Ownership

## Service lifetimes

| Service | Default lifetime | Reason |
| --- | --- | --- |
| `IDatabaseDefinitionProvider<TKey>` | Singleton | V1 definitions are static and synchronous. |
| `IDbRouter<TKey>` | Singleton | It contains an immutable validated lookup. |
| `IDbConnectionProvider` | Singleton | Providers are stateless connection constructors. |
| Provider registry | Singleton/internal | It is validated and immutable. |
| `IDatabaseSelection<TKey>` | Scoped | Selection belongs to exactly one application/DI scope. |
| `IDbConnectionFactory<TKey>` | Scoped | Its parameterless operation reads the scoped selection. |
| `IDbContextResolver<TKey,TContext>` | Scoped | It participates in the current composition scope but explicit contexts are new instances. |
| `TContext` | Scoped | Repositories in one selected scope share a unit of work. |

Singleton custom definition and connection providers must be safe for concurrent reads/calls. DbRouter does not make consumer services or EF `DbContext` thread-safe.

## DbConnection lifecycle

`IDbConnectionFactory<TKey>.Create(key)` and `.Create()` return a new, non-null, closed `DbConnection` on every successful call.

Ownership transfers immediately to the caller:

```csharp
await using DbConnection connection = factory.Create(DatabaseKey.Primary);
await connection.OpenAsync(cancellationToken);
// execute commands
```

The caller chooses when to open it and must dispose it, including when opening or command execution fails. DbRouter never caches, shares, opens, closes, or pools the connection. Pooling—if any—is the concrete ADO.NET driver's responsibility.

If provider construction throws before returning, no connection ownership transfers. DbRouter returns a sanitized `DbConnectionCreationException` without retaining the provider exception, because a third-party message can echo its connection-string input. If a faulty custom provider returns `null`, DbRouter treats that as construction failure.

## Scoped selection lifecycle

Each DI scope receives one `DatabaseSelection<TKey>`. It starts empty and is independent of every other scope. The application selects a key before resolving services that depend on selected connection/context creation.

Selection is write-once. Repeating the same key is harmless; switching keys is rejected. This protects a scope from containing services configured for inconsistent databases. Explicit `Resolve(key)`, `Create(key)`, and context creation never read or change the selection.

Disposing a scope releases its selection and all scoped services. There is no ambient state to clear.

## EF Core lifecycle

### Explicit context

Every `IDbContextResolver<TKey,TContext>.Create(key)` call returns a distinct context configured for that key. The caller owns and disposes it:

```csharp
await using ApplicationDbContext context = contextResolver.Create(DatabaseKey.Reporting);
```

The resolver does not cache explicit contexts and does not alter scoped selection.

### Scoped context

The `TContext` registration is scoped. It reads the selected key on first resolution and the container returns that same instance for subsequent resolutions in the scope. Multiple repositories therefore share the normal EF Core change tracker, transaction, and unit-of-work boundary.

The DI container owns and disposes the scoped context. Consumers must not manually dispose an injected scoped context. Separate scopes receive separate contexts, even when they select the same key.

DbRouter does not coordinate transactions between contexts or databases. A normal local transaction can be shared only through the same scoped context/connection according to EF Core behavior.

## Secret handling

Connection strings necessarily flow from definitions to construction providers, but DbRouter treats them as secrets. They are never emitted by `ToString`, exception messages, diagnostics, or library logs. Consumers remain responsible for configuration storage and for third-party driver/EF logging settings, including EF Core sensitive-data logging.
