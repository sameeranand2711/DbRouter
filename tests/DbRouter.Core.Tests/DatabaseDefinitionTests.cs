namespace DbRouter.Core.Tests;

public sealed class DatabaseDefinitionTests
{
    [Fact]
    public void ToString_redacts_connection_string()
    {
        var definition = new DatabaseDefinition<DatabaseKey>(
            DatabaseKey.Primary,
            "sqlserver",
            TestDefinitions.PrimaryConnectionString);

        string text = definition.ToString();

        Assert.Contains("sqlserver", text);
        Assert.Contains("[REDACTED]", text);
        Assert.False(text.Contains(TestDefinitions.PrimaryConnectionString, StringComparison.Ordinal));
        Assert.DoesNotContain("primary-secret", text);
    }

    [Fact]
    public void Static_provider_copies_the_input_sequence()
    {
        var definitions = new List<DatabaseDefinition<DatabaseKey>>
        {
            new(DatabaseKey.Primary, "sqlserver", TestDefinitions.PrimaryConnectionString),
        };
        var provider = new StaticDatabaseDefinitionProvider<DatabaseKey>(definitions);

        definitions.Clear();

        Assert.Single(provider.GetDefinitions());
    }
}
