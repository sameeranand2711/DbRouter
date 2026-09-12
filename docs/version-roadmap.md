# Version Roadmap

This roadmap communicates sequencing, not a promise that later features already exist.

## V1 — static routing foundation

- static database definitions
- pluggable synchronous definition provider
- strongly typed keys
- explicit and scoped selection
- provider-neutral closed `DbConnection` creation
- SQL Server and PostgreSQL proof providers
- Microsoft dependency injection
- optional explicit/scoped EF Core integration
- configuration validation, concurrency, secrecy, and lifecycle semantics

## V2 — dynamic definitions and context mapping

- dynamic database definitions
- tenant/context-based resolution
- asynchronous definition providers
- refresh and bounded caching
- secret-store adapters

## V3 — routing topology

- read/write routing
- replicas
- sharding
- region routing
- routing policies

## V4 — resilience and observability

- health-aware routing
- failover hooks
- observability
- topology changes
- advanced policies

No V2+ runtime behavior is implemented in V1. Later designs must preserve the small explicit V1 path and provider/ORM independence.
