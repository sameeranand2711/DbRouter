using System.Collections.ObjectModel;

namespace DbRouter;

/// <summary>
/// Provides an immutable snapshot of in-memory database definitions.
/// </summary>
public sealed class StaticDatabaseDefinitionProvider<TKey> : IDatabaseDefinitionProvider<TKey>
    where TKey : notnull
{
    private readonly ReadOnlyCollection<DatabaseDefinition<TKey>> _definitions;

    /// <summary>Creates a static provider by copying the supplied sequence once.</summary>
    public StaticDatabaseDefinitionProvider(IEnumerable<DatabaseDefinition<TKey>> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        _definitions = Array.AsReadOnly(definitions.ToArray());
    }

    /// <inheritdoc />
    public IReadOnlyCollection<DatabaseDefinition<TKey>> GetDefinitions() => _definitions;
}
