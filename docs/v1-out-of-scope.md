# DbRouter V1 Out of Scope

V1 deliberately excludes the following capabilities:

- dynamic configuration refresh or change tokens
- runtime tenant discovery or tenant catalogs
- asynchronous or remote definition providers
- secret retrieval, rotation, encryption, or vault adapters
- read/write splitting
- replicas, load balancing, or routing weights
- sharding or shard-map management
- region-aware, latency-aware, or health-aware routing
- automatic failover or retry policies
- topology discovery or runtime topology changes
- custom connection pooling
- distributed transactions
- repository or unit-of-work frameworks
- ORM behavior, query generation, or object mapping
- migration orchestration
- database monitoring or health checks

## Boundary clarifications

- A custom V1 definition provider may read configuration when it is constructed, but DbRouter snapshots its returned definitions once. It cannot refresh them.
- The connection factory chooses a provider from a static definition; it does not choose replicas or apply policies.
- A scoped selection is an explicitly supplied database key, not tenant discovery or ambient request inspection.
- EF integration creates/options ordinary contexts. It does not replace EF Core's transaction, tracking, pooling, retry, or migration facilities.
- SQL Server and PostgreSQL packages prove provider extensibility; their drivers retain responsibility for parsing, pooling, authentication, and network behavior.

## Roadmap placement

- V2 may address dynamic definitions, tenant/context-based resolution, async providers, refresh/caching, and secret-store adapters.
- V3 may address read/write routing, replicas, sharding, regions, and routing policies.
- V4 may address health signals, failover hooks, observability, topology changes, and advanced policies.

These roadmap labels are documentation only. No V2+ extension point may complicate the V1 API without an immediate V1 use case.
