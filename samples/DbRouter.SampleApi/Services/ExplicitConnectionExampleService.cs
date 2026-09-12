using System.Data.Common;
using DbRouter.Core.Abstractions.Connections;
using DbRouter.SampleApi.Configuration;

namespace DbRouter.SampleApi.Services;

public sealed class ExplicitConnectionExampleService
{
    private readonly IDbConnectionFactory<DatabaseKey> _connectionFactory;

    public ExplicitConnectionExampleService(
        IDbConnectionFactory<DatabaseKey> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Creates two caller-owned connections without reading or changing scoped selection.
    /// </summary>
    public (DbConnection Orders, DbConnection Payments) CreateOrdersAndPayments()
    {
        DbConnection orders = _connectionFactory.Create(DatabaseKey.Orders);

        try
        {
            DbConnection payments = _connectionFactory.Create(DatabaseKey.Payments);
            return (orders, payments);
        }
        catch
        {
            orders.Dispose();
            throw;
        }
    }
}
