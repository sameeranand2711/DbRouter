namespace DbRouter.Core.Tests;

internal sealed class ThrowingValidationDefinitionProvider : IDatabaseDefinitionProvider<DatabaseKey>
{
    public const string Secret = "validation-provider-secret";

    public IReadOnlyCollection<DatabaseDefinition<DatabaseKey>> GetDefinitions() =>
        throw new DatabaseDefinitionValidationException($"Failure containing {Secret}");
}
