namespace DbRouter.Core.Tests;

internal static class TestDefinitions
{
    public const string PrimaryConnectionString =
        "Server=primary.example;Database=app;User Id=user;Password=primary-secret";

    public const string ReportingConnectionString =
        "Host=reporting.example;Database=reports;Username=user;Password=reporting-secret";

    public static DbRouter<DatabaseKey> CreateRouter() =>
        new(
            new StaticDatabaseDefinitionProvider<DatabaseKey>(
            [
                new(DatabaseKey.Primary, "sqlserver", PrimaryConnectionString),
                new(DatabaseKey.Reporting, "postgresql", ReportingConnectionString),
            ]));
}
