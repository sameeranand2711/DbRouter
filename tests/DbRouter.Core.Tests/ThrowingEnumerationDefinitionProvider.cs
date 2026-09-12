namespace DbRouter.Core.Tests;

internal sealed class ThrowingEnumerationDefinitionProvider :
    IDatabaseDefinitionProvider<DatabaseKey>
{
    public const string Secret = "definition-enumeration-secret";

    public IReadOnlyCollection<DatabaseDefinition<DatabaseKey>> GetDefinitions() =>
        new ThrowingDefinitionCollection(Secret);
}
