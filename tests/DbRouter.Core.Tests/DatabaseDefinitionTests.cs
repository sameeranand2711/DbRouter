namespace DbRouter.Core.Tests;

public sealed class DatabaseDefinitionTests
{
    [Fact]
    public void ToString_redacts_connection_string()
    {
        const string secret = "secret-in-every-field";
        var definition = new DatabaseDefinition<string>(secret, secret, secret);

        string text = definition.ToString();

        Assert.Contains("[REDACTED]", text);
        Assert.DoesNotContain(secret, text);
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
