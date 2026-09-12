using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DbRouter.DependencyInjection.Tests;

internal sealed class ThrowingStateDbConnection : DbConnection
{
    public const string Secret = "state-inspection-secret";

    public bool IsDisposed { get; private set; }

    [AllowNull]
    public override string ConnectionString { get; set; } = string.Empty;

    public override string Database => "ThrowingStateDatabase";

    public override string DataSource => "ThrowingStateDataSource";

    public override string ServerVersion => "1.0";

    public override ConnectionState State =>
        throw new InvalidOperationException($"State failure containing {Secret}");

    public override void ChangeDatabase(string databaseName) =>
        throw new NotSupportedException();

    public override void Close() => throw new NotSupportedException();

    public override void Open() => throw new NotSupportedException();

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
        throw new NotSupportedException();

    protected override DbCommand CreateDbCommand() =>
        throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        throw new InvalidOperationException($"Dispose failure containing {Secret}");
    }
}
