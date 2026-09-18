# DbRouter Sample API

This .NET 8 minimal API is a runnable DbRouter V1 demonstration with seven logical databases across SQL Server and PostgreSQL. It covers static definition snapshots, a custom definition provider, strongly typed keys, fail-fast validation, explicit and scoped connection resolution, multiple providers, dependency injection, optional EF Core integration, lifecycle ownership, and concurrent resolution.

No credential is committed. The checked-in connection strings contain hosts, database names, and usernames only. Store passwords with .NET user-secrets or another ASP.NET Core configuration provider.

## Database layout

| Key | Database | Provider |
| --- | --- | --- |
| Primary | `company` | SQL Server |
| Customer | `company_customer` | SQL Server |
| Orders | `company_orders` | SQL Server |
| Payments | `operationguard` | PostgreSQL |
| Inventory | `operationguard_inventory` | PostgreSQL |
| Reporting | `company_reporting` | SQL Server |
| Audit | `operationguard_audit` | PostgreSQL |

The sample expects SQL Server on `localhost:1433` and PostgreSQL on `localhost:55432`. The supplied SQL Server string used `Databse`; the valid keyword is `Database`.

## Configure credentials safely

The project has a `UserSecretsId`. From the repository root, configure local credentials without writing them into Git:

~~~powershell
$sqlPassword = '<sql-server-password>'
$postgresPassword = '<postgresql-password>'
$project = 'samples/DbRouter.SampleApi/DbRouter.SampleApi.csproj'

dotnet user-secrets set 'ConnectionStrings:PrimaryDatabase' "Server=localhost;Database=company;User Id=sa;Password=$sqlPassword;TrustServerCertificate=true;Connect Timeout=5" --project $project
dotnet user-secrets set 'ConnectionStrings:CustomerDatabase' "Server=localhost;Database=company_customer;User Id=sa;Password=$sqlPassword;TrustServerCertificate=true;Connect Timeout=5" --project $project
dotnet user-secrets set 'ConnectionStrings:OrdersDatabase' "Server=localhost;Database=company_orders;User Id=sa;Password=$sqlPassword;TrustServerCertificate=true;Connect Timeout=5" --project $project
dotnet user-secrets set 'ConnectionStrings:ReportingDatabase' "Server=localhost;Database=company_reporting;User Id=sa;Password=$sqlPassword;TrustServerCertificate=true;Connect Timeout=5" --project $project

dotnet user-secrets set 'ConnectionStrings:PaymentsDatabase' "Host=localhost;Port=55432;Database=operationguard;Username=operationguard;Password=$postgresPassword;Timeout=5" --project $project
dotnet user-secrets set 'ConnectionStrings:InventoryDatabase' "Host=localhost;Port=55432;Database=operationguard_inventory;Username=operationguard;Password=$postgresPassword;Timeout=5" --project $project
dotnet user-secrets set 'ConnectionStrings:AuditDatabase' "Host=localhost;Port=55432;Database=operationguard_audit;Username=operationguard;Password=$postgresPassword;Timeout=5" --project $project

dotnet user-secrets set 'Sample:InitializeDatabases' 'true' --project $project
~~~

The PostgreSQL URL form is represented as the equivalent Npgsql keyword connection string because DbRouter passes provider-native connection strings to Npgsql.

## Provision the databases

Create the SQL Server databases while connected to `master` with an account allowed to create databases:

~~~sql
IF DB_ID(N'company') IS NULL CREATE DATABASE [company];
IF DB_ID(N'company_customer') IS NULL CREATE DATABASE [company_customer];
IF DB_ID(N'company_orders') IS NULL CREATE DATABASE [company_orders];
IF DB_ID(N'company_reporting') IS NULL CREATE DATABASE [company_reporting];
~~~

The supplied PostgreSQL server already contains `operationguard`. Create the two additional databases with the same owner:

~~~shell
createdb -U operationguard -O operationguard operationguard_inventory
createdb -U operationguard -O operationguard operationguard_audit
~~~

These commands are intentionally separate from application startup. DbRouter resolves configured databases; it does not provision servers or implement migrations.

## Static custom definition provider

`SampleDatabaseDefinitionProvider` implements `IDatabaseDefinitionProvider<DatabaseKey>` and constructs one stable seven-definition snapshot. Registration uses the actual V1 custom-provider API:

~~~csharp
services.AddDbRouter<DatabaseKey>(builder =>
{
    builder.UseDefinitionProvider<SampleDatabaseDefinitionProvider>();
    builder.AddSqlServer();
    builder.AddPostgreSql();
});
~~~

Resolving `IDbRouter<DatabaseKey>` during startup forces structural validation before the application accepts requests. This does not test network connectivity; `/api/databases/status` performs that live check.

## Explicit connection resolution

`OrderLookupRepository` injects `IDbConnectionFactory<DatabaseKey>` and deliberately asks for `DatabaseKey.Orders`. The caller receives a closed connection, opens it, and disposes it:

~~~csharp
await using DbConnection connection = connectionFactory.Create(DatabaseKey.Orders);
await connection.OpenAsync(cancellationToken);
~~~

`GET /api/connections/orders-and-payments` explicitly creates SQL Server and PostgreSQL connections in one operation. Explicit resolution is the right choice when an operation intentionally targets more than one database.

## Scoped selection and DI

For Customer routes, `CustomerDatabaseSelectionMiddleware` selects `DatabaseKey.Customer` once per request. `ScopedConnectionExampleService` receives the scoped factory through normal constructor injection and calls parameterless `Create()`.

~~~text
HTTP request
    -> middleware selects Customer
    -> scoped IDatabaseSelection<DatabaseKey>
    -> constructor-injected IDbConnectionFactory<DatabaseKey>
    -> factory.Create()
    -> closed Customer SqlConnection
~~~

DbRouter V1 intentionally does not inject a raw `DbConnection`. Factory injection keeps ownership explicit.

## EF Core modes

Only `CustomerDbContext` and `OrdersDbContext` exist; seven contexts are unnecessary.

- `GET /api/ef/orders` injects `IDbContextResolver<DatabaseKey, OrdersDbContext>`, explicitly requests Orders, and disposes the caller-owned context.
- `GET /api/ef/customers/scoped` selects Customer first, then injects a normal scoped `CustomerDbContext` into two repositories.
- Both customer repositories share the same scoped context and therefore normal EF Core tracking/unit-of-work behavior.
- With `Sample:InitializeDatabases=true`, startup uses `EnsureCreatedAsync` to create the two demonstration schemas and seed one customer, preference, and order. This is sample setup, not a migrations strategy.

## Concurrent seven-database probe

`GET /api/databases/status` creates a separate caller-owned connection for every key and probes all seven concurrently. It returns only key, provider ID, connection type, availability, and `SELECT 1`; it never returns connection strings or exception details.

This endpoint demonstrates immutable concurrent router resolution and heterogeneous provider selection. It is educational diagnostics, not a production health-check system or automatic failover feature.

## Ownership

| Object | Owner |
| --- | --- |
| `DbConnection` returned by `Create(key)` or `Create()` | Caller; open and dispose it |
| Explicit `DbContext` returned by `IDbContextResolver.Create(key)` | Caller; dispose it |
| Constructor-injected scoped `CustomerDbContext` | DI scope; do not dispose it manually |

DbRouter does not open connections, implement pooling, manage transactions, or retain successful caller-owned connections.

## Endpoints

| Endpoint | Demonstration |
| --- | --- |
| `GET /api/databases` | Strong keys, static definitions, provider mapping, no secrets |
| `GET /api/databases/status` | All seven databases and concurrent explicit resolution |
| `GET /api/connections/orders` | Explicit repository resolution |
| `GET /api/connections/orders-and-payments` | Two providers in one operation |
| `GET /api/scoped/customers` | Scoped selection and DI factory resolution |
| `GET /api/ef/orders` | Explicit caller-owned EF context |
| `GET /api/ef/customers/scoped` | Scoped EF context shared by two repositories |

## Run

User-secrets load in the Development environment:

~~~powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project samples/DbRouter.SampleApi/DbRouter.SampleApi.csproj --framework net8.0
~~~

The application can still build and run without database servers when initialization remains disabled. Network-dependent endpoints return normal unavailable responses or `available: false` results.

Run the focused tests without any database credentials:

~~~shell
dotnet test tests/DbRouter.SampleApi.Tests/DbRouter.SampleApi.Tests.csproj
~~~
