# DbRouter V1 Delivery Manifest

Workflow state: `CREATE_SAMPLE_APP = true`

| Stage | Required artifacts | State |
| --- | --- | --- |
| Agent 00 | `docs/agent-execution-status.md`, `docs/v1-delivery-manifest.md` | Complete |
| Agent 01 | Requirements, architecture, public API, lifetime/ownership, and out-of-scope documents | Complete |
| Agent 02 | ORM-independent core, validation, scoped selection, resolution and concurrency tests | Complete |
| Agent 03 | DbConnection factory, provider registry, SQL Server and PostgreSQL packages, DI, tests | Complete |
| Agent 04 | Optional EF Core package, explicit/scoped context creation, lifecycle tests | Complete |
| Agent 05 | Consumer validation, documentation set, release checklist, release report | Complete |
| Final delivery | Open PR/MR targeting `main` | Complete: [PR #2](https://github.com/sameeranand2711/DbRouter/pull/2) open for human review |

## Completed artifacts

### Agent 00

- Established an empty `main` baseline without implementing on `main`.
- Created the dedicated `chore/v1-orchestration` branch.
- Recorded sequential execution and verification gates.
- Recorded `CREATE_SAMPLE_APP = UNDECIDED` without inferring a decision.

### Agent 01

- `docs/v1-requirements.md`
- `docs/v1-architecture.md`
- `docs/v1-public-api.md`
- `docs/v1-lifetime-and-ownership.md`
- `docs/v1-out-of-scope.md`
- Approved Core, DI, provider, and optional EF Core package boundaries.
- Approved explicit and write-once scoped selection semantics.
- Approved caller-owned closed connections and container/caller-owned EF context modes.

### Agent 02

- Solution/build infrastructure: `DbRouter.sln`, `Directory.Build.props`, and `Directory.Packages.props`.
- Core package: `src/DbRouter.Core` targeting `net8.0;net10.0` with no external packages.
- Definition API: `DatabaseDefinition<TKey>`, `IDatabaseDefinitionProvider<TKey>`, and `StaticDatabaseDefinitionProvider<TKey>`.
- Resolution API: `IDbRouter<TKey>` and immutable `DbRouter<TKey>` lookup.
- Selection API: `IDatabaseSelection<TKey>` and concurrency-safe, write-once `DatabaseSelection<TKey>`.
- Connection contracts: `IDbConnectionProvider` and `IDbConnectionFactory<TKey>`; implementations remain assigned to Agent 03.
- Safe definition, resolution, and selection exception types.
- Core test project with 28 passing tests on each supported framework, including validation, secrecy, custom keys, explicit/scoped independence, DI-scope isolation, and concurrent reads.

### Agent 03

- DI package: `src/DbRouter.DependencyInjection` with fluent registration, custom definition/provider support, immutable provider registry, and scoped connection factory.
- SQL Server package: `src/DbRouter.SqlServer` using `Microsoft.Data.SqlClient`.
- PostgreSQL package: `src/DbRouter.PostgreSql` using `Npgsql`.
- Safe provider-not-found, provider-registration, and connection-creation exception behavior in Core.
- Direct and factory-mediated provider construction failures are sanitized without retaining potentially secret-bearing third-party exceptions.
- DI/factory test project with 18 passing tests on each framework.
- Concrete provider test project with 7 passing tests on each framework and no live database dependency.
- Mixed providers, closed state, explicit/scoped creation, custom providers, duplicates, missing providers, disposal, leakage, and concurrency are covered.

### Agent 04

- Optional EF package: `src/DbRouter.EntityFrameworkCore`, targeting `net8.0;net10.0`.
- Explicit context API: `IDbContextResolver<TKey,TContext>` returning a distinct caller-owned context.
- Scoped context registration: one selected `TContext` instance shared through conventional constructor injection in a DI scope.
- Provider-neutral options registry through `DbRouterEntityFrameworkBuilder<TContext>` callbacks.
- Safe EF provider-registration, provider-not-found, and context-creation exceptions.
- Target-matched EF Core dependencies: 8.0.31 for `net8.0`, 10.0.12 for `net10.0`.
- EF test project with 15 passing tests on each framework covering explicit/scoped configuration, disposal, repository injection, unit-of-work sharing, selection independence, provider independence, and secret-safe failures.

### Agent 05

- Completed consumer validation for ADO.NET, Dapper-compatible access, explicit/scoped EF Core, repositories, and mixed providers.
- Reviewed and reduced the public surface; strengthened definition and provider failure sanitization.
- Added the required README, license, consumer guides, roadmap, release checklist, and release report.
- Added a focused dual-framework in-process performance harness under `benchmarks/DbRouter.Benchmarks`.
- Final clean Release build: zero warnings and zero errors.
- Final tests: 30 Core, 19 DI/factory, 7 provider, and 15 EF tests per framework; 71 per framework and 142 combined, with zero failures or skips.
- Built and inspected version 1.0.0-rc.1 packages for Core, DI, SQL Server, PostgreSQL, and optional EF Core, each with `net8.0` and `net10.0` assets.
- NuGet direct/transitive advisory checks reported no vulnerable package in the five library projects.
- Agent 05 result: `STATUS: PASS`.

## V1 framework policy

- Core and integration packages target `net8.0` and `net10.0`.
- `net6.0` is excluded.
- Any unavoidable target restriction must be documented and justified.

## Sample application

Complete on the dedicated release-candidate branch after an explicit user decision changed the gate to `true`.

- `samples/DbRouter.SampleApi`: .NET 8 minimal Web API with seven heterogeneous database registrations.
- Explicit and scoped `DbConnection` examples with caller-owned disposal.
- Explicit caller-owned and scoped DI-owned EF Core context examples using ordinary contexts.
- Customer selection middleware that runs before scoped repository/context resolution.
- Focused `tests/DbRouter.SampleApi.Tests` coverage that requires no active database.
