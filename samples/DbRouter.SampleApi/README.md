# DbRouter Sample API

This .NET 8 minimal Web API demonstrates how a consuming application registers and uses seven logical databases through DbRouter. It is intentionally an educational sample, not a production architecture.

The application compiles and starts without database servers. Endpoints that open connections naturally return a generic 503 response until the corresponding safe local-development connection string points to an available database. No endpoint returns a connection string.

## Registration

Configuration contains seven named connection strings:

| Database key | Configuration name | Provider |
| --- | --- | --- |
| Primary | PrimaryDatabase | SQL Server |
| Customer | CustomerDatabase | SQL Server |
| Orders | OrdersDatabase | SQL Server |
| Payments | PaymentsDatabase | PostgreSQL |
| Inventory | InventoryDatabase | PostgreSQL |
| Reporting | ReportingDatabase | SQL Server |
| Audit | AuditDatabase | PostgreSQL |

The composition root maps them explicitly in Extensions/ServiceCollectionExtensions.cs:

~~~csharp
services.AddDbRouter<DatabaseKey>(builder =>
{
    builder.AddSqlServer(
        DatabaseKey.Primary,
        configuration.GetConnectionString("PrimaryDatabase")!);
    builder.AddSqlServer(
        DatabaseKey.Customer,
        configuration.GetConnectionString("CustomerDatabase")!);
    // Orders, Payments, Inventory, Reporting, and Audit follow the same pattern.
});
~~~

Only the composition root reads IConfiguration. Repositories and application services receive typed DbRouter dependencies and never receive connection strings or IServiceProvider.

## Explicit DbConnection resolution

OrderLookupRepository injects IDbConnectionFactory<DatabaseKey> and deliberately asks for the Orders connection:

~~~csharp
await using var connection =
    connectionFactory.Create(DatabaseKey.Orders);

await connection.OpenAsync(cancellationToken);
~~~

Use explicit resolution when the operation itself knows its target. GET /api/connections/orders demonstrates this path.

## Multiple databases in one operation

ExplicitConnectionExampleService obtains both Orders and Payments connections by key. GET /api/connections/orders-and-payments opens, uses, and disposes both.

Explicit resolution is preferable here because one scoped selection represents only one database. Calling Create(key) does not read or change the scoped selection.

## Scoped resolution

CustomerDatabaseSelectionMiddleware selects DatabaseKey.Customer before the matching endpoint is invoked:

~~~text
HTTP request
    -> CustomerDatabaseSelectionMiddleware
    -> IDatabaseSelection<DatabaseKey>.Select(Customer)
    -> endpoint and downstream scoped services
    -> IDbConnectionFactory<DatabaseKey>.Create()
~~~

GET /api/scoped/customers uses ScopedConnectionExampleService. The service injects the factory and calls parameterless Create(), so it uses the selected Customer database without receiving a key or connection string.

## DI resolution

DbRouter V1 does not register a raw DbConnection for constructor injection. The closest supported equivalent is constructor injection of IDbConnectionFactory<DatabaseKey>:

~~~csharp
public ScopedConnectionExampleService(
    IDbConnectionFactory<DatabaseKey> connectionFactory)
{
    _connectionFactory = connectionFactory;
}
~~~

This keeps connection ownership explicit: the consuming method creates, opens, and disposes each connection.

## Explicit EF Core resolution

OrdersDbContext is an ordinary EF Core context with a standard DbContextOptions<OrdersDbContext> constructor. It has no DbRouter-specific base class.

GET /api/ef/orders injects IDbContextResolver<DatabaseKey, OrdersDbContext> and creates a context explicitly:

~~~csharp
await using OrdersDbContext context =
    resolver.Create(DatabaseKey.Orders);
~~~

This mode is suitable when an operation deliberately names a database. Every returned context is distinct and caller-owned.

## Scoped EF Core resolution

GET /api/ef/customers/scoped follows this flow:

~~~text
HTTP request
    -> select DatabaseKey.Customer
    -> DbRouter scoped resolution
    -> DI constructs one CustomerDbContext
    -> CustomerRepository and CustomerPreferenceRepository
~~~

Both repositories use normal constructor injection:

~~~csharp
public CustomerRepository(CustomerDbContext dbContext)
{
    _dbContext = dbContext;
}
~~~

They know nothing about DbRouter internals or database configuration. The test project verifies functionally that changes staged by both repositories appear in the same scoped context change tracker, preserving normal EF Core unit-of-work behavior.

The sample configures EF only for SQL Server because its two demonstration contexts target Customer and Orders. PostgreSQL remains demonstrated through DbConnection routing for Payments, Inventory, and Audit; the sample does not create seven context classes.

## Ownership

| Object | Owner |
| --- | --- |
| DbConnection returned by Create(key) or Create() | Caller; open and dispose it |
| DbContext returned by IDbContextResolver.Create(key) | Caller; dispose it |
| CustomerDbContext injected by DI | DI scope; do not dispose it manually |

DbRouter creates closed connections. It does not open, share, cache, pool, or dispose successful caller-owned connections.

## Endpoints

| Endpoint | Demonstration |
| --- | --- |
| GET /api/databases | Seven logical keys; never connection strings |
| GET /api/connections/orders | Explicit Orders DbConnection |
| GET /api/connections/orders-and-payments | SQL Server and PostgreSQL in one operation |
| GET /api/scoped/customers | Scoped Customer selection and factory injection |
| GET /api/ef/orders | Explicit caller-owned OrdersDbContext |
| GET /api/ef/customers/scoped | Selected, normally injected CustomerDbContext |

## Run

~~~shell
dotnet run --project samples/DbRouter.SampleApi/DbRouter.SampleApi.csproj
~~~

The appsettings.json values are non-production local placeholders. Replace them through normal ASP.NET Core configuration for local testing. Never commit real credentials or log complete connection strings.

Run the focused tests without starting a database:

~~~shell
dotnet test tests/DbRouter.SampleApi.Tests/DbRouter.SampleApi.Tests.csproj
~~~
