# DbConnection Usage

## Explicit ADO.NET

```csharp
await using DbConnection connection = factory.Create(DatabaseKey.Primary);
await connection.OpenAsync(cancellationToken);

await using DbCommand command = connection.CreateCommand();
command.CommandText = "select current_timestamp";
object? value = await command.ExecuteScalarAsync(cancellationToken);
```

`Create(key)` always uses the requested key and never changes scoped selection.

## Scoped ADO.NET

```csharp
selection.Select(DatabaseKey.Reporting);

await using DbConnection connection = factory.Create();
await connection.OpenAsync(cancellationToken);
```

Parameterless `Create()` throws `DatabaseSelectionMissingException` until a key is selected.

## Dapper compatibility

DbRouter does not reference Dapper. A returned `DbConnection` naturally supports Dapper extension methods in a consumer that installs Dapper:

```csharp
await using DbConnection connection = factory.Create(DatabaseKey.Reporting);

IEnumerable<ReportRow> rows = await connection.QueryAsync<ReportRow>(
    new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
```

Dapper opens a closed connection when appropriate according to its own behavior. Ownership still belongs to the caller.

## Mixed providers

The same factory can return different concrete types:

```text
DatabaseKey.Primary   -> SqlConnection
DatabaseKey.Reporting -> NpgsqlConnection
```

The mapping comes from registered providers; there is no central provider switch.

## Failure behavior

- unknown key: `DatabaseNotFoundException`
- no provider registration: `DbConnectionProviderNotFoundException`
- duplicate/invalid provider registration: `DbConnectionProviderRegistrationException`
- provider returns null/open or throws: sanitized `DbConnectionCreationException`

Provider construction exceptions are not retained as inner exceptions because third-party messages may echo connection strings.
