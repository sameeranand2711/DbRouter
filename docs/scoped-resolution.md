# Scoped Resolution

Scoped selection expresses “this DI scope uses database X.” It is explicit state stored in `IDatabaseSelection<TKey>`, not ambient/static context.

```text
DI scope created
    -> Select(valid key)
        -> Resolve repositories/context/factory
            -> Dispose scope
```

## Rules

- A new scope starts without a selection.
- `Select(key)` validates the key against the router.
- Repeating the same key is idempotent.
- Selecting another key throws `DatabaseSelectionConflictException`.
- Reading before selection throws `DatabaseSelectionMissingException`; `TryGetSelected` returns false.
- Separate scopes are isolated, including under concurrent use.
- Explicit router, connection, and context operations do not modify the scope.

Write-once behavior prevents an already-created scoped `DbContext` from disagreeing with a later key change.

## Request/job boundary

Select the database near the start of the request or job, before resolving database-dependent services. In ASP.NET Core this can be middleware or an endpoint filter; in a worker it can be immediately after creating the job scope. The application determines the key—DbRouter V1 does not discover tenants or inspect requests.

## Thread safety

Selection reads/writes are synchronized, but a selected EF `DbContext` is not thread-safe. Do not run parallel EF operations on one scoped context.
