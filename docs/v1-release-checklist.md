# DbRouter V1 Release Checklist

## Functionality

- [x] Static definitions work.
- [x] Custom definition providers work.
- [x] Strongly typed keys work.
- [x] Explicit resolution works.
- [x] Scoped resolution works and is isolated.
- [x] `DbConnection` factory returns closed caller-owned connections.
- [x] SQL Server and PostgreSQL providers remain independently referenceable.
- [x] Custom connection providers work.
- [x] EF integration is optional and provider-neutral.
- [x] EF scoped context lifetime and conventional repository injection work.
- [x] EF explicit context resolution and disposal work.

## Quality and security

- [x] Concurrent resolution and connection creation tests pass.
- [x] Concurrent DI scopes retain isolated selections.
- [x] Definition/provider lookups are immutable after initialization.
- [x] Configuration values are redacted from `ToString()` and DbRouter exceptions.
- [x] Potentially secret-bearing custom/third-party exceptions are sanitized.
- [x] No `IServiceProvider` is retained as a runtime service locator.
- [x] Public API was reviewed; unnecessary `DbRouterBuilder.Services` was removed.
- [x] Core has no EF or concrete-provider dependency.
- [x] Non-EF packages do not pull EF Core transitively.
- [x] Provider and EF dependency directions were reviewed.
- [x] V2+ runtime functionality was not implemented.

## Compatibility and delivery

- [x] `net8.0` restore/build/tests pass.
- [x] `net10.0` restore/build/tests pass.
- [x] All library packages build for both frameworks.
- [x] NuGet package construction succeeds.
- [x] Package dependency graphs were reviewed.
- [x] Local package vulnerability audit completed without a reported vulnerable package.
- [x] Required documentation is complete.
- [x] Sample application created after `CREATE_SAMPLE_APP` was explicitly set to `true`.
- [ ] Final PR/MR opened against `main` — pending remote configuration.

The sample gate is complete. Agent 05 has completed its technical release-readiness gate. Agent 00 still cannot close final delivery until the required PR/MR can be created.
