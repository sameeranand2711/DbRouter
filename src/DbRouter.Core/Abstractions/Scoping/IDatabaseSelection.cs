using System.Diagnostics.CodeAnalysis;

namespace DbRouter.Core.Abstractions.Scoping;

/// <summary>
/// Holds the write-once database key selected for one application scope.
/// </summary>
public interface IDatabaseSelection<TKey>
    where TKey : notnull
{
    /// <summary>Gets whether this scope has a selection.</summary>
    bool HasSelection { get; }

    /// <summary>Gets the selected key or throws when no key has been selected.</summary>
    TKey SelectedKey { get; }

    /// <summary>Selects a valid key for this scope.</summary>
    void Select(TKey key);

    /// <summary>Attempts to get the selected key.</summary>
    bool TryGetSelected([MaybeNullWhen(false)] out TKey key);
}
