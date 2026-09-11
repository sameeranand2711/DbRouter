using System.Data.Common;

namespace DbRouter.Core.Abstractions.Connections;

/// <summary>
/// Creates provider-appropriate closed database connections.
/// </summary>
public interface IDbConnectionFactory<TKey>
    where TKey : notnull
{
    /// <summary>Creates a connection for an explicit key.</summary>
    DbConnection Create(TKey key);

    /// <summary>Creates a connection for the current scoped selection.</summary>
    DbConnection Create();
}
