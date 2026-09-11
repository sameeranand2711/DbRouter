# EF Core Usage

EF Core support is optional. Reference `DbRouter.EntityFrameworkCore` and the EF database-provider packages your application uses. Match the provider packages to the application's EF Core major version.

## Ordinary context

No DbRouter base class is required:

```csharp
public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
```

## Register provider options

```csharp
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

The callbacks live in the application composition root, where provider-specific EF extension packages are already known. DbRouter's EF package itself references neither provider.

## Scoped context mode

Select a key before first resolving `ApplicationDbContext` or a repository that injects it:

```csharp
selection.Select(DatabaseKey.Primary);
OrderRepository repository = scope.ServiceProvider.GetRequiredService<OrderRepository>();
```

The container creates one context for that scope and disposes it with the scope. All repositories injecting `ApplicationDbContext` share the same change tracker and normal local transaction/unit-of-work boundary.

Do not manually dispose an injected scoped context. Do not change selection after scoped services have been resolved; selection is write-once and rejects changes.

## Explicit context mode

```csharp
await using ApplicationDbContext reporting =
    contextResolver.Create(DatabaseKey.Reporting);
```

Each call creates a distinct caller-owned context. This mode is suitable for intentional multi-database operations. It does not read or modify scoped selection.

## Transactions

DbRouter adds no transaction abstraction. Repositories sharing one scoped context can use normal EF Core local transactions. Contexts for different keys are independent, and V1 provides no distributed transaction coordination.

## Sensitive data

DbRouter sanitizes its construction errors, but EF/provider logging is controlled by the application. Do not enable EF Core sensitive-data logging in environments where values must remain confidential.
