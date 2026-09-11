# DbRouter V1 Release Report

## Recommendation

`STATUS: PASS` for Agent 05 technical release readiness.

DbRouter V1 is ready for human review. The implementation, documentation, dual-target validation, package construction, dependency review, concurrency checks, secrecy checks, and representative performance measurements are complete. The repository still has no configured Git remote, so Agent 00 cannot create the required open PR/MR until a remote is supplied. That delivery-system blocker does not change the technical Agent 05 result.

## Implemented functionality

- Static `DatabaseDefinition<TKey>` snapshots and a synchronous pluggable `IDatabaseDefinitionProvider<TKey>`.
- Strongly typed keys using `TKey : notnull` and the key type's default equality semantics.
- Immutable, concurrency-safe explicit resolution through `IDbRouter<TKey>`.
- Write-once, concurrency-safe selection isolated to a DI scope.
- Explicit-key and selected-key `DbConnection` creation.
- Closed, caller-owned connection semantics with safe failure handling.
- Provider registration without a central provider switch.
- Independently referenceable SQL Server and PostgreSQL providers.
- Microsoft dependency injection integration.
- Optional, provider-neutral EF Core explicit and scoped context integration.
- Startup snapshot validation and secret-safe library exceptions.

## Architecture summary

`DbRouter.Core` owns only definitions, resolution, selection, connection contracts, and safe exceptions. It has no external package dependencies. `DbRouter.DependencyInjection` depends on Core and Microsoft DI abstractions. Concrete ADO.NET providers depend inward on Core/DI. `DbRouter.EntityFrameworkCore` depends inward on Core/DI and EF Core, but no Core or non-EF package depends on it.

Definitions are materialized and validated into an immutable lookup. A scoped selection stores a validated key only within its owning DI scope. The connection factory resolves a definition, finds a provider in an immutable case-insensitive registry, and returns a closed connection. The EF resolver uses the same definition and an immutable provider-options registry to construct either a distinct caller-owned context or one container-owned scoped context.

No runtime component retains `IServiceProvider`; DI factory delegates are used only at the composition boundary. No ambient/static selection state, dynamic refresh path, provider switch, ORM behavior, repository framework, or custom pooling exists.

## Package list and dependency review

| Package | Direct dependency boundary |
| --- | --- |
| `DbRouter.Core` | None |
| `DbRouter.DependencyInjection` | Core; Microsoft DI abstractions |
| `DbRouter.SqlServer` | Core/DI; `Microsoft.Data.SqlClient` 7.0.2 |
| `DbRouter.PostgreSql` | Core/DI; `Npgsql` 10.0.3 |
| `DbRouter.EntityFrameworkCore` | Core/DI; target-matched EF Core |

The .NET 8 asset uses EF Core 8.0.31 and Microsoft DI 8.x. The .NET 10 asset uses EF Core 10.0.12 and Microsoft DI 10.x. The provider packages do not depend on EF Core, and the EF package does not depend on a concrete EF database provider. A NuGet advisory query covering direct and transitive dependencies reported no vulnerable packages in any of the five library projects on 2026-09-12.

Five `1.0.0` NuGet packages build successfully and contain `net8.0` and `net10.0` library/XML documentation assets plus the package README. Package dependency groups were inspected for both target frameworks.

## Public API review

The public surface is limited to consumer contracts, direct non-DI implementations, composition builders/extensions, concrete provider entry points, and typed failure categories:

- Core definition, provider, router, selection, and connection interfaces are the required cross-package contracts.
- `DatabaseDefinition<TKey>`, `StaticDatabaseDefinitionProvider<TKey>`, `DbRouter<TKey>`, and `DatabaseSelection<TKey>` remain public for applications using custom containers or no DI container.
- DI and provider builders/extensions are public composition APIs. The unnecessary `DbRouterBuilder.Services` escape hatch was removed.
- SQL Server and PostgreSQL connection providers remain public for direct use and custom-container registration.
- `IDbContextResolver<TKey,TContext>` and the EF builder/registration extension form the optional EF consumer surface.
- Public exceptions allow callers to distinguish configuration, lookup, selection, registration, and creation failures without parsing messages.

All generic key APIs use `TKey : notnull`; relevant outputs carry nullable-flow attributes. Each primary public type has its own file. XML documentation is generated for packages. Connection/context ownership is documented on the APIs and in the consumer guides.

Connection strings are never included in library-generated exception messages. `DatabaseDefinition<TKey>.ToString()` redacts all configuration-bearing values. Failures from custom definition providers, provider identifiers, ADO.NET constructors, EF callbacks, and context factories are replaced at trust boundaries without retaining potentially secret-bearing inner exceptions.

## Consumer and concurrency validation

The automated suite validates:

- ADO.NET-style explicit and scoped connection usage without referencing EF Core.
- Dapper-compatible use of the returned ordinary `DbConnection` abstraction without adding Dapper to Core.
- Explicit caller-owned EF contexts.
- Scoped container-owned EF contexts injected into an ordinary repository, including shared tracking/unit-of-work behavior.
- A mixed SQL Server/PostgreSQL definition set without a central switch.
- Concurrent router resolution and connection creation.
- Concurrent reads of a selected key and isolated selections across separate DI scopes.
- Disposal ownership and rejection/disposal of invalid provider results.
- Configuration validation and connection-string secrecy across failure paths.

## Build and test results

Final Release validation on 2026-09-12:

| Gate | Result |
| --- | --- |
| Clean solution restore | PASS |
| Release build | PASS, 0 warnings, 0 errors |
| `net8.0` tests | PASS, 71 passed, 0 failed, 0 skipped |
| `net10.0` tests | PASS, 71 passed, 0 failed, 0 skipped |
| Combined tests | PASS, 142 passed |
| NuGet package construction | PASS, five packages |
| Direct/transitive vulnerability query | PASS, none reported |

Per target framework, the 71 tests comprise 30 Core, 19 DI/connection-factory, 7 provider, and 15 EF Core tests.

## Performance observations

The small Release harness measures in-process library paths and excludes database network latency. The latest representative run on the validation machine produced:

| Path | .NET 8 | .NET 10 |
| --- | ---: | ---: |
| Key resolution | 46.99 ns/op | 20.89 ns/op |
| Scoped selection lookup | 43.18 ns/op | 26.48 ns/op |
| Provider lookup plus `SqlConnection` construction/disposal | 2,437.14 ns/op | 2,989.84 ns/op |

These are indicative stopwatch measurements, not stable cross-machine performance promises. They show no reason to add complexity to the immutable lookup design.

## Framework compatibility

All five library packages target exactly `net8.0;net10.0`. No `net6.0` target or framework restriction was introduced. Both assets restore, compile, test, and pack successfully with target-matched Microsoft dependencies.

## Deferred features

V1 intentionally excludes dynamic refresh, tenant discovery, asynchronous/remote providers, read/write routing, replicas, sharding, region routing, health routing, load balancing, failover, secret management, custom connection pooling, repositories, ORM behavior, migrations, and distributed transactions. The documented V2-V4 roadmap describes possible sequencing only; none of that runtime behavior is present.

## Known limitations

- Definitions are a static in-process snapshot. A custom provider is called when the singleton router is first activated, so applications wanting fail-fast startup should resolve the router during startup validation.
- Validation checks structure and registration, not database reachability or credentials.
- Connections are returned closed. Opening, commands, transactions, and disposal belong to the caller.
- Explicit EF contexts are caller-owned; the one scoped EF context is container-owned and still follows EF Core's normal non-thread-safe rule.
- EF provider configuration callbacks and concrete EF provider packages belong to the consuming application.
- Sanitizing untrusted provider/driver failures intentionally omits their inner exceptions; diagnose detailed driver failures in a controlled environment without logging secrets.
- Repository URL metadata and the required PR/MR cannot be populated/created until this repository has a Git remote.

## Sample application status

`CREATE_SAMPLE_APP = UNDECIDED`. Per the workflow gate, no sample was created and the deliverable remains pending a human decision. This does not make V1 technically unready.

## Final delivery status

Agent 05 passes. Agent 00 must still create an open PR/MR targeting `main` and leave it unmerged. That final action is externally blocked because `git remote -v` is empty; configure a remote before closing the overall workflow.
