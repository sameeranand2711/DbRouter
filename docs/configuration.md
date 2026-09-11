# Configuration

## Inline static definitions

The fluent path is the simplest V1 configuration:

```csharp
services.AddDbRouter<DatabaseKey>(builder =>
{
    builder.AddSqlServer(DatabaseKey.Primary, primaryConnectionString);
    builder.AddPostgreSql(DatabaseKey.Reporting, reportingConnectionString);
});
```

The provider extensions register both their connection provider and the inline definition. To use one provider for definitions supplied elsewhere, call parameterless `AddSqlServer()` or `AddPostgreSql()`.

## IConfiguration remains optional

DbRouter Core does not depend on `IConfiguration` or `appsettings.json`. A composition root may read any configuration system and pass values to the builder:

```csharp
string primary = configuration.GetConnectionString("Primary")
    ?? throw new InvalidOperationException("Primary connection is not configured.");

services.AddDbRouter<DatabaseKey>(builder =>
    builder.AddSqlServer(DatabaseKey.Primary, primary));
```

This keeps environment variables, JSON, vault hydration, and other configuration choices outside Core. V1 does not fetch or rotate secrets.

## Custom definition provider

Implement a static synchronous provider:

```csharp
public sealed class ApplicationDatabaseDefinitions
    : IDatabaseDefinitionProvider<DatabaseKey>
{
    private readonly IReadOnlyCollection<DatabaseDefinition<DatabaseKey>> _definitions;

    public ApplicationDatabaseDefinitions(IConfiguration configuration)
    {
        _definitions =
        [
            new(
                DatabaseKey.Primary,
                SqlServerDbConnectionProvider.Id,
                configuration.GetConnectionString("Primary")
                    ?? throw new InvalidOperationException("Primary is missing.")),
        ];
    }

    public IReadOnlyCollection<DatabaseDefinition<DatabaseKey>> GetDefinitions() => _definitions;
}
```

Register it instead of inline definitions:

```csharp
services.AddDbRouter<DatabaseKey>(builder =>
{
    builder.UseDefinitionProvider<ApplicationDatabaseDefinitions>();
    builder.AddSqlServer();
});
```

Inline definitions and a custom definition provider cannot be mixed. A custom provider is registered as a singleton and must return a stable snapshot. Resolve `IDbRouter<TKey>` during startup to force validation.

## Validation rules

- keys are non-null and unique under default key equality;
- provider IDs are non-empty and match providers case-insensitively;
- connection strings are non-empty;
- concrete providers perform their own parsing when a connection is constructed;
- duplicate connection or EF provider IDs are rejected;
- errors and `ToString()` output never include configuration values.

Provider-specific semantic validation does not open a database connection. Connectivity and authentication remain runtime concerns.
