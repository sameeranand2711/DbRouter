using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace DbRouter.SqlServer;

/// <summary>Creates closed Microsoft SQL Server connections.</summary>
public sealed class SqlServerDbConnectionProvider : IDbConnectionProvider
{
    /// <summary>The stable provider identifier used in database definitions.</summary>
    public const string Id = "sqlserver";

    /// <inheritdoc />
    public string ProviderId => Id;

    /// <inheritdoc />
    public DbConnection Create(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        try
        {
            return new SqlConnection(connectionString);
        }
        catch (Exception)
        {
            throw new DbConnectionCreationException();
        }
    }
}
