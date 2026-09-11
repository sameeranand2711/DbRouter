# Provider Development

Implement `IDbConnectionProvider` to add an ADO.NET driver without changing Core or the factory.

```csharp
public sealed class AcmeDbConnectionProvider : IDbConnectionProvider
{
    public const string Id = "acmedb";

    public string ProviderId => Id;

    public DbConnection Create(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        try
        {
            return new AcmeConnection(connectionString);
        }
        catch (Exception)
        {
            throw new DbConnectionCreationException();
        }
    }
}
```

## Contract

- Use a stable, non-empty provider ID. IDs are compared with `StringComparer.OrdinalIgnoreCase`.
- Be stateless and safe for concurrent `Create` calls; providers are singletons.
- Return a new, non-null, closed `DbConnection` each time.
- Do not open, pool, cache, or share connections.
- Do not log the connection string.
- Sanitize driver-constructor failures because driver messages may include input.
- If an open connection is returned, DbRouter disposes and rejects it.

## Fluent registration extension

```csharp
public static class AcmeDbRouterBuilderExtensions
{
    public static DbRouterBuilder<TKey> AddAcme<TKey>(
        this DbRouterBuilder<TKey> builder)
        where TKey : notnull =>
        builder.AddProvider<AcmeDbConnectionProvider>();

    public static DbRouterBuilder<TKey> AddAcme<TKey>(
        this DbRouterBuilder<TKey> builder,
        TKey key,
        string connectionString)
        where TKey : notnull =>
        builder
            .AddAcme()
            .AddDatabase(key, AcmeDbConnectionProvider.Id, connectionString);
}
```

Keep the provider in its own package with references to `DbRouter.Core`, `DbRouter.DependencyInjection`, and the concrete driver. Unit tests should assert concrete type, closed state, repeated/concurrent construction, disposal ownership, invalid strings, and error secrecy without requiring a running database.

## EF Core provider options

EF integration is separate. Applications can register a callback:

```csharp
efBuilder.AddProvider(
    AcmeDbConnectionProvider.Id,
    (options, connectionString) => options.UseAcme(connectionString));
```

A provider package may add a fluent extension over `DbRouterEntityFrameworkBuilder<TContext>`. Do not add provider references to the central EF integration.
