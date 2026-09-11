# Getting Started

## Requirements

- .NET 8 or .NET 10 SDK/runtime
- Microsoft dependency injection for the standard registration path
- one or more ADO.NET provider packages, or custom `IDbConnectionProvider` implementations
- optional EF Core provider packages only when EF integration is used

## Install

For a mixed SQL Server/PostgreSQL ADO.NET application:

```shell
dotnet add package DbRouter.DependencyInjection
dotnet add package DbRouter.SqlServer
dotnet add package DbRouter.PostgreSql
```

Add `DbRouter.EntityFrameworkCore` only if the application uses EF Core. Applications using ADO.NET, Dapper, or custom data access do not need it and do not receive EF Core transitively.

## Define keys and register targets

```csharp
using DbRouter.PostgreSql;
using DbRouter.SqlServer;

public enum DatabaseKey
{
    Primary,
    Reporting,
}

services.AddDbRouter<DatabaseKey>(builder =>
{
    builder.AddSqlServer(DatabaseKey.Primary, primaryConnectionString);
    builder.AddPostgreSql(DatabaseKey.Reporting, reportingConnectionString);
});
```

Keys may also be strings, GUIDs, record structs, or other non-null value objects with stable equality. Duplicate keys, missing provider IDs, and empty connection strings fail validation.

## Choose explicit or scoped use

Explicit creation is appropriate when the operation itself knows its target:

```csharp
await using var connection = factory.Create(DatabaseKey.Reporting);
await connection.OpenAsync(cancellationToken);
```

Scoped creation is appropriate when a request/job chooses one target for downstream services:

```csharp
selection.Select(DatabaseKey.Primary);
await using var connection = factory.Create();
await connection.OpenAsync(cancellationToken);
```

The connection is closed when returned and must be disposed by the caller. Select before resolving a scoped EF context or another service that captures the selected target.

## Validate custom configuration during startup

Inline definitions are validated when the singleton router is first constructed. Ensure custom providers are also activated during application startup:

```csharp
_ = app.Services.GetRequiredService<IDbRouter<DatabaseKey>>();
```

In tests or hosts, enabling service-provider validation also catches lifetime/construction mistakes, but application startup should still resolve the router to validate provider data.

Continue with [configuration](configuration.md), [DbConnection usage](dbconnection-usage.md), or [EF Core usage](efcore-usage.md).
