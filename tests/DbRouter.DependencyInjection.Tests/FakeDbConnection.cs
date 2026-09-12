using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DbRouter.DependencyInjection.Tests;

internal sealed class FakeDbConnection : DbConnection
{
    private ConnectionState _state;

    public FakeDbConnection(string connectionString, ConnectionState state = ConnectionState.Closed)
    {
        ConnectionString = connectionString;
        _state = state;
    }

    public bool IsDisposed { get; private set; }

    [AllowNull]
    public override string ConnectionString { get; set; }

    public override string Database => "FakeDatabase";

    public override string DataSource => "FakeDataSource";

    public override string ServerVersion => "1.0";

    public override ConnectionState State => _state;

    public override void ChangeDatabase(string databaseName) =>
        throw new NotSupportedException();

    public override void Close() => _state = ConnectionState.Closed;

    public override void Open() => _state = ConnectionState.Open;

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
        throw new NotSupportedException();

    protected override DbCommand CreateDbCommand() =>
        throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        _state = ConnectionState.Closed;
        base.Dispose(disposing);
    }
}
