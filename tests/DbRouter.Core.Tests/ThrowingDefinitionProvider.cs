namespace DbRouter.Core.Tests;

internal sealed class ThrowingDefinitionProvider : IDatabaseDefinitionProvider<DatabaseKey>
{
    public const string Secret = "definition-provider-secret";

    public IReadOnlyCollection<DatabaseDefinition<DatabaseKey>> GetDefinitions() =>
        throw new InvalidOperationException($"Failure containing {Secret}");
}
