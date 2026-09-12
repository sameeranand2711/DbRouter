# Concepts

## Database definition

`DatabaseDefinition<TKey>` is an immutable V1 tuple of key, provider ID, and connection string. It deliberately has no tenant, replica, region, health, routing-policy, or metadata bag. `ToString()` redacts all configuration.

## Definition provider

`IDatabaseDefinitionProvider<TKey>` returns one synchronous, static snapshot. `StaticDatabaseDefinitionProvider<TKey>` copies an in-memory sequence. The router calls a custom provider once during its own construction; V1 has no refresh or asynchronous retrieval path.

## Router

`IDbRouter<TKey>` resolves definitions explicitly through `Resolve` and `TryResolve`. `DbRouter<TKey>` validates the snapshot and converts it to a frozen dictionary using `EqualityComparer<TKey>.Default`. Concurrent reads do not mutate shared state.

## Scoped selection

`IDatabaseSelection<TKey>` holds one key in one DI scope. It is initially empty, validates the first key, accepts an idempotent repeat, and rejects switching to a different key. It uses a private lock and no global/static/`AsyncLocal` state.

Selection is a convenience for downstream scoped services; explicit resolution remains available and never changes selection.

## Connection provider and factory

An `IDbConnectionProvider` maps one stable provider ID to connection-object construction. The factory uses an immutable case-insensitive registry, not a switch statement. Provider implementations must be stateless/concurrency-safe and return a new closed connection.

`IDbConnectionFactory<TKey>.Create(key)` is explicit; parameterless `Create()` uses scoped selection. Both transfer connection ownership to the caller.

## EF Core resolver

The optional `IDbContextResolver<TKey,TContext>` creates explicit contexts. A separate scoped `TContext` registration uses scoped selection and is shared by conventional repositories in that scope. EF options delegates isolate provider-specific calls from the central package.

## Package boundaries

Core depends only on the BCL. DI points to Core. ADO.NET providers point to Core/DI and their drivers. EF integration points to Core/DI and EF Core. No dependency points from Core or DI back to EF or a concrete driver.
