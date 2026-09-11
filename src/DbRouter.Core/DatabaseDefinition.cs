namespace DbRouter;

/// <summary>
/// Describes one statically configured database target.
/// </summary>
/// <typeparam name="TKey">The non-null database key type.</typeparam>
public sealed class DatabaseDefinition<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Initializes a database definition. The definition set is validated when a router is created.
    /// </summary>
    public DatabaseDefinition(TKey key, string providerId, string connectionString)
    {
        Key = key;
        ProviderId = providerId;
        ConnectionString = connectionString;
    }

    /// <summary>Gets the database key.</summary>
    public TKey Key { get; }

    /// <summary>Gets the provider identifier.</summary>
    public string ProviderId { get; }

    /// <summary>
    /// Gets the connection string. Treat this value as a secret and never include it in diagnostics.
    /// </summary>
    public string ConnectionString { get; }

    /// <inheritdoc />
    public override string ToString() =>
        $"{nameof(DatabaseDefinition<TKey>)} {{ Key = {Key}, ProviderId = {ProviderId}, ConnectionString = [REDACTED] }}";
}
