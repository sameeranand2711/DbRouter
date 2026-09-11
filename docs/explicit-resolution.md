# Explicit Resolution

Explicit resolution is the right mode when an operation deliberately names its database target or needs more than one target.

## Definition lookup

```csharp
DatabaseDefinition<DatabaseKey> definition = router.Resolve(DatabaseKey.Primary);

if (router.TryResolve(candidate, out DatabaseDefinition<DatabaseKey>? optional))
{
    // use the known definition
}
```

`Resolve` throws `DatabaseNotFoundException` for an unknown key. `TryResolve` returns false only for an unknown key; a null reference key remains invalid.

Treat `ConnectionString` as a secret. Prefer the connection/context factories instead of reading it in ordinary application code.

## Explicit connection

```csharp
await using DbConnection connection = factory.Create(DatabaseKey.Reporting);
await connection.OpenAsync(cancellationToken);
```

The returned connection is closed and caller-owned.

## Explicit EF context

```csharp
await using ApplicationDbContext context =
    resolver.Create(DatabaseKey.Reporting);
```

Each context is distinct and caller-owned. Explicit context creation does not alter the database selected for scoped repositories.
