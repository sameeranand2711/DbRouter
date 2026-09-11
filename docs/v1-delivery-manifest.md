# DbRouter V1 Delivery Manifest

Workflow state: `CREATE_SAMPLE_APP = UNDECIDED`

| Stage | Required artifacts | State |
| --- | --- | --- |
| Agent 00 | `docs/agent-execution-status.md`, `docs/v1-delivery-manifest.md` | Complete |
| Agent 01 | Requirements, architecture, public API, lifetime/ownership, and out-of-scope documents | Pending |
| Agent 02 | ORM-independent core, validation, scoped selection, resolution and concurrency tests | Pending |
| Agent 03 | DbConnection factory, provider registry, SQL Server and PostgreSQL packages, DI, tests | Pending |
| Agent 04 | Optional EF Core package, explicit/scoped context creation, lifecycle tests | Pending |
| Agent 05 | Consumer validation, documentation set, release checklist, release report | Pending |
| Final delivery | Open PR/MR targeting `main` | Blocked until a Git remote is configured |

## Completed artifacts

### Agent 00

- Established an empty `main` baseline without implementing on `main`.
- Created the dedicated `chore/v1-orchestration` branch.
- Recorded sequential execution and verification gates.
- Recorded `CREATE_SAMPLE_APP = UNDECIDED` without inferring a decision.

## V1 framework policy

- Core and integration packages target `net8.0` and `net10.0`.
- `net6.0` is excluded.
- Any unavoidable target restriction must be documented and justified.

## Sample application

Pending human decision. Agent 05 must not create a sample while the value remains `UNDECIDED`; this does not block technical release readiness.
