using System.Data.Common;
using Npgsql;

namespace DbRouter.PostgreSql;

/// <summary>Creates closed PostgreSQL connections through Npgsql.</summary>
public sealed class PostgreSqlDbConnectionProvider : IDbConnectionProvider
{
    /// <summary>The stable provider identifier used in database definitions.</summary>
    public const string Id = "postgresql";

    /// <inheritdoc />
    public string ProviderId => Id;

    /// <inheritdoc />
    public DbConnection Create(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        try
        {
            return new NpgsqlConnection(connectionString);
        }
        catch (Exception)
        {
            throw new DbConnectionCreationException();
        }
    }
}
