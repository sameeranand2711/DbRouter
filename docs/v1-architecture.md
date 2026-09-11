# DbRouter V1 Architecture

## Design summary

V1 separates target resolution, scoped selection, ADO.NET connection construction, and optional EF Core construction. The core contains no ORM or concrete database-provider dependency.

```text
Consumer
  |-- explicit key ---------------------------+
  |                                           |
  +-- scoped IDatabaseSelection<TKey> --------+--> IDbRouter<TKey>
                                                   |
                                                   +--> DatabaseDefinition<TKey>
                                                            |
                      +-------------------------------------+-------------------+
                      |                                                         |
             IDbConnectionFactory<TKey>                              IDbContextResolver<TKey,TContext>
                      |                                                         |
             provider-id registry                                   EF options-configurator registry
                      |                                                         |
             IDbConnectionProvider                                      ordinary TContext
```

Explicit operations read the requested key directly. Scoped operations read a key stored only in the current DI scope. Neither path changes the other.

## Packages and dependency direction

```text
DbRouter.Core
  ^          ^                     ^
  |          |                     |
DbRouter.DependencyInjection       DbRouter.EntityFrameworkCore --> Microsoft.EntityFrameworkCore
  ^          ^
  |          |
DbRouter.SqlServer --> Microsoft.Data.SqlClient
DbRouter.PostgreSql --> Npgsql
```

- `DbRouter.Core` references only framework/BCL assemblies, including `System.Data.Common`.
- `DbRouter.DependencyInjection` references Core and Microsoft DI abstractions.
- Each ADO.NET provider package references Core, DI for registration extensions, and exactly its concrete driver.
- `DbRouter.EntityFrameworkCore` references Core, DI, and EF Core. Core and non-EF packages never reference it.
- EF database-provider packages are consumer choices. The generic EF integration accepts provider-specific options configurators rather than hardwiring SQL Server or PostgreSQL.

## Repository structure

```text
/
|-- src/
|   |-- DbRouter.Core/
|   |-- DbRouter.DependencyInjection/
|   |-- DbRouter.SqlServer/
|   |-- DbRouter.PostgreSql/
|   `-- DbRouter.EntityFrameworkCore/
|-- tests/
|   |-- DbRouter.Core.Tests/
|   |-- DbRouter.DependencyInjection.Tests/
|   |-- DbRouter.Provider.Tests/
|   `-- DbRouter.EntityFrameworkCore.Tests/
|-- docs/
|-- samples/                         # only when CREATE_SAMPLE_APP=true
|-- Directory.Build.props
|-- Directory.Packages.props
|-- DbRouter.sln
`-- README.md
```

Projects multi-target `net8.0;net10.0`. Central package management fixes dependency versions. Nullable reference types, implicit usings, deterministic builds, package metadata, and warnings-as-errors for library source are configured centrally.

## Definition and resolution flow

`IDatabaseDefinitionProvider<TKey>` supplies a static, synchronous collection. `DbRouter<TKey>` consumes the provider once during construction, validates every definition, and materializes a read-only dictionary using `EqualityComparer<TKey>.Default`. There is no refresh path in V1.

`Resolve` is the throwing API. `TryResolve` is the non-throwing API for an absent key. Invalid provider data always fails initialization rather than appearing as a normal missing lookup.

The router is registered as a singleton because its state is immutable after construction. Custom providers must therefore describe static V1 configuration and are normally singletons.

## Scoped selection

`DatabaseSelection<TKey>` is a small state holder registered once per DI scope behind `IDatabaseSelection<TKey>`. It has no static or `AsyncLocal` state. Selection is write-once per scope:

- the first valid key is stored;
- selecting the same key again is idempotent;
- selecting a different key throws `DatabaseSelectionConflictException`;
- reading before selection throws `DatabaseSelectionMissingException`;
- selection validates the key through `IDbRouter<TKey>` before storing it.

Key equality uses `EqualityComparer<TKey>.Default`, matching the router. Write-once behavior prevents a scoped `DbContext` or other scoped dependency from silently disagreeing with a later selection change. Internal synchronization makes concurrent access deterministic, although callers should normally select at the scope boundary before resolving data-access services.

## Connection construction

`IDbConnectionFactory<TKey>` supports explicit `Create(key)` and scoped `Create()` calls. It resolves the definition, looks up an `IDbConnectionProvider` by provider identifier, and asks it to construct a connection.

Provider lookup is a validated, case-insensitive dictionary built once. There is no provider switch statement. SQL Server and PostgreSQL packages register their own implementations and identifiers. Custom providers use the same contract.

Every returned connection is closed. Ownership transfers to the caller for explicit/manual use; when a connection is registered or held by another consumer, that consumer's normal ownership rules apply. DbRouter neither opens nor pools connections.

## Dependency injection composition

`AddDbRouter<TKey>` configures exactly one definition source, zero or more provider implementations, a singleton router/registries, and scoped selection/connection services. Static definitions are the default source. A custom `IDatabaseDefinitionProvider<TKey>` replaces that source and cannot be combined with inline definitions, avoiding ambiguous precedence.

Provider packages expose fluent extensions both to register only a provider and to register a provider plus an inline static definition. Configuration errors are rejected during registration where possible and otherwise when the affected singleton is first constructed.

Runtime services receive typed dependencies. They do not receive or retain `IServiceProvider`; standard DI registration factories may resolve dependencies only at the composition boundary.

## EF Core integration

EF Core construction has two modes:

- `IDbContextResolver<TKey,TContext>.Create(key)` creates a new explicit context. It does not mutate scoped selection; the caller owns and disposes it.
- A scoped `TContext` registration reads `IDatabaseSelection<TKey>` once, creates one context, and lets the DI scope own it. Repositories inject `TContext` conventionally and receive the same instance within the scope.

The integration builds `DbContextOptions<TContext>` for the resolved definition through a case-insensitive registry of provider-specific configuration delegates. The consumer (or a separate provider-specific extension package) supplies calls such as `UseSqlServer` or `UseNpgsql`; the central package contains neither. Contexts need only the ordinary `DbContextOptions<TContext>` constructor, supplied through a typed activation delegate at registration.

The EF project uses the target-matched EF Core major version for each target framework while keeping one source API. It does not introduce a library-specific base context or repository pattern.

## Failure boundaries

- Bad definition sets fail router initialization with `DatabaseDefinitionValidationException`.
- Missing keys fail with `DatabaseNotFoundException`.
- Missing/conflicting scope state has selection-specific exceptions.
- Missing/duplicate connection providers have provider-specific exceptions.
- A concrete driver's construction error is replaced by a sanitized DbRouter exception without retaining a potentially secret-bearing inner exception.
- EF options/configuration and activation failures are wrapped without connection-string data.

Exception messages identify only the failed operation and never include a connection string. Provider construction exceptions are not retained because third-party messages can echo their input. This is a deliberate security correction to the initial design; consumers can diagnose provider configuration by validating it outside secret-bearing production paths.

## Concurrency model

Definition and provider dictionaries are built once and never mutated. Concurrent router reads and factory calls use only local variables and immutable shared data. Scoped selection uses a private lock and lives in one DI scope. EF scoped contexts are not made thread-safe by DbRouter; EF Core's normal single-operation-at-a-time rule remains in force.

## Architecture decisions

- Synchronous provider APIs match V1's static configuration and avoid fake async APIs.
- `TKey : notnull` supports value objects without imposing enum/string restrictions.
- No metadata bag is included because V1 has no justified consumer for it.
- No `AsyncLocal` is used because DI scopes provide explicit, testable isolation.
- A closed connection is the least surprising ownership boundary and lets consumers choose when/how to open it.
- Scope selection is write-once to preserve consistency of already-created scoped services.
