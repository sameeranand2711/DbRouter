# Troubleshooting

## DatabaseNotFoundException

The requested or selected key is absent. Check key equality, enum/value-object values, and whether the definition provider returned the complete startup snapshot. Exception text intentionally does not stringify arbitrary keys.

## DatabaseDefinitionValidationException

The snapshot contains a null/malformed definition, duplicate key, missing provider ID, empty connection string, or the custom provider failed. DbRouter sanitizes the error, so inspect configuration at the composition root without logging secrets.

## DatabaseSelectionMissingException

Select a database before calling parameterless connection creation or resolving a scoped EF context:

```csharp
selection.Select(DatabaseKey.Primary);
```

Ensure selection happens before a repository/context is first resolved.

## DatabaseSelectionConflictException

The scope already selected another key. Do not reuse one DI scope for unrelated database selections. Create separate scopes or use explicit `Create(key)` operations.

## DbConnectionProviderNotFoundException

The definition's provider ID has no registered `IDbConnectionProvider`. Add the concrete provider with `AddSqlServer`, `AddPostgreSql`, or `AddProvider`. IDs are case-insensitive but not trimmed.

## DbConnectionProviderRegistrationException

One provider has an empty/throwing ID, or multiple implementations use the same ID. Register one implementation per ID. Repeated generic registration of the same concrete provider is deduplicated.

## DbConnectionCreationException

The provider threw, returned null, or returned an already-open connection. The exception is deliberately sanitized and has no provider inner exception. Validate the connection string with the concrete driver in a controlled development environment, taking care not to print credentials.

## DbContextProviderNotFoundException

The EF builder has no options callback matching the definition's provider ID. Register every provider used by EF contexts.

## DbContextCreationException

An EF options callback or typed context factory failed/returned null. The exception is sanitized. Verify the callback, target-matched EF provider package, and ordinary `DbContextOptions<TContext>` constructor.

## Restore/build network failures

NuGet audit or repository-signature checks require access to configured package sources. Retry restore when the source is reachable, then build/test with `--no-restore`. Do not suppress audit warnings in release validation.

## EF concurrency errors

DbRouter's lookup/selection is concurrency-safe; EF `DbContext` is not. Do not run parallel operations on the same scoped context. Create independent scopes or explicit contexts.
