# Lifetime and Disposal

| Object/service | Lifetime/owner | Disposal rule |
| --- | --- | --- |
| definition provider | singleton | container owns it if disposable |
| router | singleton | immutable lookup; no owned connections |
| connection provider | singleton | stateless; container owns it if disposable |
| scoped selection | one per DI scope | discarded with scope |
| connection factory | one per DI scope | holds no connection |
| created `DbConnection` | caller | caller disposes every returned connection |
| explicit EF context | caller | caller disposes every returned context |
| injected scoped EF context | DI scope | container disposes; consumer must not |

## Closed connection contract

Factories return closed connections so the ownership boundary is visible:

```csharp
await using DbConnection connection = factory.Create(key);
await connection.OpenAsync(cancellationToken);
```

Opening, commands, transactions, and failures after return are the caller's responsibility. DbRouter does not implement connection pooling; the ADO.NET driver may pool internally.

## Scope consistency

Select before resolving scoped database services. The selected key cannot change to another value, preventing different services in one scope from capturing different databases. Separate scopes never share a selection or EF context.

## EF unit of work

Repositories injecting the same scoped context share normal EF state and local transactions. Explicit contexts and contexts in different scopes are independent. DbRouter does not coordinate distributed transactions.

## Failure before ownership transfer

When construction fails or a provider returns null/open, no object is returned and ownership does not transfer. An open rejected connection is disposed by DbRouter. Sanitized construction exceptions omit provider inner exceptions to prevent secret leakage.

See the normative design notes in [V1 lifetime and ownership](v1-lifetime-and-ownership.md).
