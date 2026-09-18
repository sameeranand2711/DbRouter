namespace DbRouter.SampleApi.Configuration;

public sealed class SampleDatabaseOptions
{
    public SampleDatabaseOptions(
        string primaryConnectionString,
        string customerConnectionString,
        string ordersConnectionString,
        string paymentsConnectionString,
        string inventoryConnectionString,
        string reportingConnectionString,
        string auditConnectionString)
    {
        PrimaryConnectionString = primaryConnectionString;
        CustomerConnectionString = customerConnectionString;
        OrdersConnectionString = ordersConnectionString;
        PaymentsConnectionString = paymentsConnectionString;
        InventoryConnectionString = inventoryConnectionString;
        ReportingConnectionString = reportingConnectionString;
        AuditConnectionString = auditConnectionString;
    }

    public string PrimaryConnectionString { get; }

    public string CustomerConnectionString { get; }

    public string OrdersConnectionString { get; }

    public string PaymentsConnectionString { get; }

    public string InventoryConnectionString { get; }

    public string ReportingConnectionString { get; }

    public string AuditConnectionString { get; }

    public override string ToString() =>
        $"{nameof(SampleDatabaseOptions)} {{ Configuration = [REDACTED] }}";
}
