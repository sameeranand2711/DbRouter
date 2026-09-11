# DbRouter V1 Requirements

## Purpose

DbRouter V1 is a small .NET library for selecting a configured database target and creating the appropriate ADO.NET or optional Entity Framework Core object. It supports applications that use ADO.NET, Dapper, custom data-access code, EF Core, or a mixture of those approaches.

DbRouter owns routing configuration and object construction. It does not own query execution, connection pooling, repositories, migrations, secrets, or distributed transactions.

## Supported frameworks

All V1 library and integration packages target both `net8.0` and `net10.0`. Shared public APIs remain identical across targets. Target-matched dependency versions may differ where a dependency's major version is tied to a target framework; such conditions must be visible in project files and release documentation.

`net6.0` is not supported.

## Functional requirements

V1 must provide:

1. Immutable database definitions containing a non-null key, provider identifier, and connection string.
2. Generic non-null keys, including enums, strings, GUIDs, strongly typed IDs, and custom value objects.
3. A synchronous definition-provider abstraction and an in-memory static implementation.
4. Explicit lookup by key through `Resolve` and `TryResolve` without changing scoped state.
5. A per-DI-scope database selection with predictable missing-selection and conflicting-selection behavior.
6. A provider-neutral factory that returns a closed `DbConnection` selected explicitly or from the current scope.
7. Independently referenceable SQL Server and PostgreSQL proof-provider packages.
8. Dependency-injection registration that also accepts custom definition and connection providers.
9. An optional EF Core package supporting explicit context creation and conventional scoped `DbContext` injection.
10. Startup/initialization validation for malformed definitions and duplicate keys or provider registrations.
11. Immutable, concurrency-safe lookup structures after initialization and isolation between DI scopes.
12. Explicit caller/container ownership and disposal rules.

## Validation and security requirements

- Keys, provider identifiers, and connection strings must be validated before use.
- Duplicate keys are invalid under the configured key comparer.
- Provider identifiers are non-empty and compared using `StringComparer.OrdinalIgnoreCase`.
- Connection strings must be non-empty or non-whitespace. Provider-specific parsing remains the provider's responsibility.
- Unknown keys and providers fail with documented DbRouter exceptions.
- Connection construction failures preserve an inner exception but never add the connection string to a message.
- Definitions must redact the connection string from `ToString()`.
- DbRouter must never write connection strings to logs, exceptions, diagnostics, or test output.

## Quality requirements

- The common resolution path is dictionary-backed and does not repeatedly enumerate definitions.
- Concurrent reads and connection construction are supported after initialization.
- Mutable static or ambient selection state is forbidden.
- Each primary public type has its own source file.
- Builds and tests must pass for `net8.0` and `net10.0` after each implementation stage.
- Non-EF consumers must not receive EF Core transitively.

## Sample application gate

`CREATE_SAMPLE_APP = UNDECIDED`. Library work continues, but Agent 05 must not create a sample until a human changes this value to `true` or `false`.
