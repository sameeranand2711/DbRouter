namespace DbRouter.Core.Tests;

internal sealed class NullCollectionDefinitionProvider : IDatabaseDefinitionProvider<DatabaseKey>
{
    public IReadOnlyCollection<DatabaseDefinition<DatabaseKey>> GetDefinitions() => null!;
}
