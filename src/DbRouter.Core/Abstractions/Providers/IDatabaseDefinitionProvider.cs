using DbRouter.Core.Models;

namespace DbRouter.Core.Abstractions.Providers;

/// <summary>
/// Supplies the static database definitions used to initialize a router.
/// </summary>
public interface IDatabaseDefinitionProvider<TKey>
    where TKey : notnull
{
    /// <summary>Gets the complete V1 definition snapshot.</summary>
    IReadOnlyCollection<DatabaseDefinition<TKey>> GetDefinitions();
}
