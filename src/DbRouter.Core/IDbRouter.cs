using System.Diagnostics.CodeAnalysis;

namespace DbRouter;

/// <summary>
/// Resolves validated database definitions by strongly typed key.
/// </summary>
public interface IDbRouter<TKey>
    where TKey : notnull
{
    /// <summary>Resolves a definition or throws when the key is unknown.</summary>
    DatabaseDefinition<TKey> Resolve(TKey key);

    /// <summary>Attempts to resolve a definition without throwing for an unknown key.</summary>
    bool TryResolve(
        TKey key,
        [NotNullWhen(true)] out DatabaseDefinition<TKey>? definition);
}
