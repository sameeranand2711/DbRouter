using System.Data.Common;

namespace DbRouter.Core.Abstractions.Providers;

/// <summary>
/// Constructs closed ADO.NET connections for one provider identifier.
/// </summary>
public interface IDbConnectionProvider
{
    /// <summary>Gets the stable provider identifier.</summary>
    string ProviderId { get; }

    /// <summary>Creates a new closed connection. Ownership transfers to the caller.</summary>
    DbConnection Create(string connectionString);
}
