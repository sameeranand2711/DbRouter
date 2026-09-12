namespace DbRouter.Core.Tests;

public sealed class DbRouterValidationTests
{
    [Fact]
    public void Duplicate_keys_are_rejected_without_leaking_definitions()
    {
        var provider = new StaticDatabaseDefinitionProvider<DatabaseKey>(
        [
            new(DatabaseKey.Primary, "sqlserver", TestDefinitions.PrimaryConnectionString),
            new(DatabaseKey.Primary, "postgresql", TestDefinitions.ReportingConnectionString),
        ]);

        DatabaseDefinitionValidationException exception =
            Assert.Throws<DatabaseDefinitionValidationException>(() => new DbRouter<DatabaseKey>(provider));

        Assert.Contains("duplicate key", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("primary-secret", exception.ToString());
        Assert.DoesNotContain("reporting-secret", exception.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Missing_provider_identifier_is_rejected(string? providerId)
    {
        var provider = new StaticDatabaseDefinitionProvider<DatabaseKey>(
        [
            new(DatabaseKey.Primary, providerId!, TestDefinitions.PrimaryConnectionString),
        ]);

        DatabaseDefinitionValidationException exception =
            Assert.Throws<DatabaseDefinitionValidationException>(() => new DbRouter<DatabaseKey>(provider));

        Assert.Contains("provider identifier", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("primary-secret", exception.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Empty_connection_string_is_rejected(string? connectionString)
    {
        var provider = new StaticDatabaseDefinitionProvider<DatabaseKey>(
        [
            new(DatabaseKey.Primary, "sqlserver", connectionString!),
        ]);

        DatabaseDefinitionValidationException exception =
            Assert.Throws<DatabaseDefinitionValidationException>(() => new DbRouter<DatabaseKey>(provider));

        Assert.Contains("empty connection string", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Null_definition_is_rejected()
    {
        var provider = new StaticDatabaseDefinitionProvider<DatabaseKey>(
        [
            null!,
        ]);

        DatabaseDefinitionValidationException exception =
            Assert.Throws<DatabaseDefinitionValidationException>(() => new DbRouter<DatabaseKey>(provider));

        Assert.Contains("index 0", exception.Message);
    }

    [Fact]
    public void Null_definition_collection_is_rejected()
    {
        DatabaseDefinitionValidationException exception =
            Assert.Throws<DatabaseDefinitionValidationException>(
                () => new DbRouter<DatabaseKey>(new NullCollectionDefinitionProvider()));

        Assert.Contains("null collection", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Null_definition_provider_is_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => new DbRouter<DatabaseKey>(null!));
    }

    [Fact]
    public void Definition_provider_failure_is_sanitized()
    {
        DatabaseDefinitionValidationException exception =
            Assert.Throws<DatabaseDefinitionValidationException>(
                () => new DbRouter<DatabaseKey>(new ThrowingDefinitionProvider()));

        Assert.DoesNotContain(ThrowingDefinitionProvider.Secret, exception.ToString());
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Definition_provider_validation_failure_is_sanitized()
    {
        DatabaseDefinitionValidationException exception =
            Assert.Throws<DatabaseDefinitionValidationException>(
                () => new DbRouter<DatabaseKey>(new ThrowingValidationDefinitionProvider()));

        Assert.DoesNotContain(ThrowingValidationDefinitionProvider.Secret, exception.ToString());
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Definition_provider_enumeration_failure_is_sanitized()
    {
        DatabaseDefinitionValidationException exception =
            Assert.Throws<DatabaseDefinitionValidationException>(
                () => new DbRouter<DatabaseKey>(new ThrowingEnumerationDefinitionProvider()));

        Assert.DoesNotContain(ThrowingEnumerationDefinitionProvider.Secret, exception.ToString());
        Assert.Null(exception.InnerException);
    }
}
