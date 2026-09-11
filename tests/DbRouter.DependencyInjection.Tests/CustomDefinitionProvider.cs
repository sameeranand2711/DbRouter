namespace DbRouter.DependencyInjection.Tests;

internal sealed class CustomDefinitionProvider : IDatabaseDefinitionProvider<DatabaseKey>
{
    public const string ConnectionString = "custom-provider-secret";

    public IReadOnlyCollection<DatabaseDefinition<DatabaseKey>> GetDefinitions() =>
    [
        new(DatabaseKey.Reporting, "fake", ConnectionString),
    ];
}
