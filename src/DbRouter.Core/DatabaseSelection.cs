using System.Diagnostics.CodeAnalysis;

namespace DbRouter;

/// <summary>
/// Implements isolated, concurrency-safe, write-once database selection.
/// </summary>
public sealed class DatabaseSelection<TKey> : IDatabaseSelection<TKey>
    where TKey : notnull
{
    private readonly object _sync = new();
    private readonly IDbRouter<TKey> _router;
    private bool _hasSelection;
    private TKey _selectedKey = default!;

    /// <summary>Creates a selection that validates keys through the supplied router.</summary>
    public DatabaseSelection(IDbRouter<TKey> router)
    {
        ArgumentNullException.ThrowIfNull(router);
        _router = router;
    }

    /// <inheritdoc />
    public bool HasSelection
    {
        get
        {
            lock (_sync)
            {
                return _hasSelection;
            }
        }
    }

    /// <inheritdoc />
    public TKey SelectedKey
    {
        get
        {
            lock (_sync)
            {
                return _hasSelection
                    ? _selectedKey
                    : throw new DatabaseSelectionMissingException();
            }
        }
    }

    /// <inheritdoc />
    public void Select(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _router.Resolve(key);

        lock (_sync)
        {
            if (!_hasSelection)
            {
                _selectedKey = key;
                _hasSelection = true;
                return;
            }

            if (!EqualityComparer<TKey>.Default.Equals(_selectedKey, key))
            {
                throw new DatabaseSelectionConflictException();
            }
        }
    }

    /// <inheritdoc />
    public bool TryGetSelected([MaybeNullWhen(false)] out TKey key)
    {
        lock (_sync)
        {
            key = _selectedKey;
            return _hasSelection;
        }
    }
}
