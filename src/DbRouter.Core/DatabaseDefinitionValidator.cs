namespace DbRouter;

internal static class DatabaseDefinitionValidator
{
    public static void Validate<TKey>(DatabaseDefinition<TKey>? definition, int index)
        where TKey : notnull
    {
        if (definition is null)
        {
            throw new DatabaseDefinitionValidationException(
                $"The database definition at index {index} is null.");
        }

        if (definition.Key is null)
        {
            throw new DatabaseDefinitionValidationException(
                $"The database definition at index {index} has a null key.");
        }

        if (string.IsNullOrWhiteSpace(definition.ProviderId))
        {
            throw new DatabaseDefinitionValidationException(
                $"The database definition at index {index} has a missing provider identifier.");
        }

        if (string.IsNullOrWhiteSpace(definition.ConnectionString))
        {
            throw new DatabaseDefinitionValidationException(
                $"The database definition at index {index} has an empty connection string.");
        }
    }
}
