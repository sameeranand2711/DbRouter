# DbRouter

DbRouter is a lightweight .NET abstraction for selecting database targets and creating the appropriate database-access object in applications with multiple database connections.

V1 provides static definitions, strongly typed keys, explicit and scoped selection, provider-neutral `DbConnection` creation, Microsoft dependency injection, SQL Server and PostgreSQL providers, and optional EF Core integration. Packages target `net8.0` and `net10.0`.

DbRouter is not an ORM, repository framework, connection pool, migration framework, secret manager, distributed transaction manager, or database monitoring product.

## Packages

| Package | Purpose |
| --- | --- |
| `DbRouter.Core` | Definitions, resolution, scoped-selection and connection contracts; no external packages. |
| `DbRouter.DependencyInjection` | Microsoft DI registration and provider-neutral connection factory. |
| `DbRouter.SqlServer` | `Microsoft.Data.SqlClient` connection provider. |
| `DbRouter.PostgreSql` | `Npgsql` connection provider. |
| `DbRouter.EntityFrameworkCore` | Optional, provider-neutral EF Core context integration. |

Install Core/DI and only the provider packages the application needs:

```shell
dotnet add package DbRouter.DependencyInjection
dotnet add package DbRouter.SqlServer
dotnet add package DbRouter.PostgreSql
```

## Register databases

```csharp
using DbRouter.DependencyInjection.Extensions;
using DbRouter.PostgreSql.Extensions;
using DbRouter.SqlServer.Extensions;

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

Definitions are validated once and resolved from an immutable, concurrency-safe lookup. Provider IDs are case-insensitive. V1 configuration is static; it does not refresh at runtime.

## Create a connection explicitly

```csharp
using System.Data.Common;

await using DbConnection connection = factory.Create(DatabaseKey.Reporting);
await connection.OpenAsync(cancellationToken);
```

Every returned connection is closed and caller-owned. DbRouter never opens, shares, pools, or disposes it for you.

## Use a scoped selection

Select before resolving database-dependent scoped services:

```csharp
selection.Select(DatabaseKey.Primary);

await using DbConnection connection = factory.Create();
await connection.OpenAsync(cancellationToken);
```

Selection is isolated to one DI scope and is write-once. Explicit operations do not mutate it.

## Add optional EF Core support

Install `DbRouter.EntityFrameworkCore` and the EF provider packages selected by the application. DbRouter does not pull SQL Server or PostgreSQL EF providers into the integration package.

```csharp
using DbRouter.EntityFrameworkCore.Extensions;
using DbRouter.PostgreSql.Providers;
using DbRouter.SqlServer.Providers;

services.AddDbRouterEntityFrameworkCore<DatabaseKey, ApplicationDbContext>(
    options => new ApplicationDbContext(options),
    builder =>
    {
        builder.AddProvider(
            SqlServerDbConnectionProvider.Id,
            (options, connectionString) => options.UseSqlServer(connectionString));
        builder.AddProvider(
            PostgreSqlDbConnectionProvider.Id,
            (options, connectionString) => options.UseNpgsql(connectionString));
    });
```

After a key is selected, repositories inject the ordinary scoped context:

```csharp
public sealed class OrderRepository(ApplicationDbContext db)
{
    private readonly ApplicationDbContext _db = db;
}
```

For deliberate cross-database work, inject `IDbContextResolver<DatabaseKey, ApplicationDbContext>` and call `Create(key)`. Explicit contexts are caller-owned; scoped contexts are container-owned.

## Documentation

- [Getting started](docs/getting-started.md)
- [Concepts](docs/concepts.md)
- [Configuration](docs/configuration.md)
- [DbConnection usage](docs/dbconnection-usage.md)
- [EF Core usage](docs/efcore-usage.md)
- [Lifetime and disposal](docs/lifetime-and-disposal.md)
- [Provider development](docs/provider-development.md)
- [Troubleshooting](docs/troubleshooting.md)
- [Version roadmap](docs/version-roadmap.md)
- [Comprehensive sample API](samples/DbRouter.SampleApi/README.md)

The sample API demonstrates seven live-capable logical databases, a custom static definition provider, concurrent multi-provider probing, explicit/scoped ADO.NET access, and explicit/scoped EF Core access without requiring database servers to compile or test.
