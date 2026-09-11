using Microsoft.Extensions.DependencyInjection;

namespace DbRouter.DependencyInjection.Tests;

public sealed class RegistrationTests
{
    [Fact]
    public void Registers_singleton_router_and_scoped_selection_and_factory()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddProvider(new FakeDbConnectionProvider());
            builder.AddDatabase(DatabaseKey.Primary, "fake", "safe-test-value");
        });
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        using IServiceScope firstScope = provider.CreateScope();
        using IServiceScope secondScope = provider.CreateScope();

        IDbRouter<DatabaseKey> firstRouter =
            firstScope.ServiceProvider.GetRequiredService<IDbRouter<DatabaseKey>>();
        IDbRouter<DatabaseKey> secondRouter =
            secondScope.ServiceProvider.GetRequiredService<IDbRouter<DatabaseKey>>();
        IDatabaseSelection<DatabaseKey> firstSelection =
            firstScope.ServiceProvider.GetRequiredService<IDatabaseSelection<DatabaseKey>>();
        IDatabaseSelection<DatabaseKey> secondSelection =
            secondScope.ServiceProvider.GetRequiredService<IDatabaseSelection<DatabaseKey>>();
        IDbConnectionFactory<DatabaseKey> firstFactory =
            firstScope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();
        IDbConnectionFactory<DatabaseKey> secondFactory =
            secondScope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>();

        Assert.Same(firstRouter, secondRouter);
        Assert.NotSame(firstSelection, secondSelection);
        Assert.NotSame(firstFactory, secondFactory);
    }

    [Fact]
    public void Custom_definition_provider_is_supported()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.UseDefinitionProvider<CustomDefinitionProvider>();
            builder.AddProvider(new FakeDbConnectionProvider());
        });
        using ServiceProvider provider = services.BuildServiceProvider();

        IDbRouter<DatabaseKey> router = provider.GetRequiredService<IDbRouter<DatabaseKey>>();

        Assert.Equal(DatabaseKey.Reporting, router.Resolve(DatabaseKey.Reporting).Key);
    }

    [Fact]
    public void Inline_definition_then_custom_provider_is_rejected()
    {
        var services = new ServiceCollection();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => services.AddDbRouter<DatabaseKey>(builder =>
            {
                builder.AddDatabase(DatabaseKey.Primary, "fake", "safe-test-value");
                builder.UseDefinitionProvider<CustomDefinitionProvider>();
            }));

        Assert.Contains("cannot be combined", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Custom_provider_then_inline_definition_is_rejected()
    {
        var services = new ServiceCollection();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => services.AddDbRouter<DatabaseKey>(builder =>
            {
                builder.UseDefinitionProvider<CustomDefinitionProvider>();
                builder.AddDatabase(DatabaseKey.Primary, "fake", "safe-test-value");
            }));

        Assert.Contains("cannot be combined", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Duplicate_provider_identifiers_are_rejected_case_insensitively()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddProvider(new FakeDbConnectionProvider("duplicate"));
            builder.AddProvider(new FakeDbConnectionProvider("DUPLICATE"));
            builder.AddDatabase(DatabaseKey.Primary, "duplicate", "safe-test-value");
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        DbConnectionProviderRegistrationException exception =
            Assert.Throws<DbConnectionProviderRegistrationException>(
                () => scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>());

        Assert.Contains("same provider identifier", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Missing_provider_identifier_is_rejected()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddProvider(new MissingIdDbConnectionProvider());
            builder.AddDatabase(DatabaseKey.Primary, "fake", "safe-test-value");
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        Assert.Throws<DbConnectionProviderRegistrationException>(
            () => scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<DatabaseKey>>());
    }

    [Fact]
    public void Duplicate_database_keys_are_rejected_when_router_initializes()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
        {
            builder.AddDatabase(DatabaseKey.Primary, "fake", "first-secret");
            builder.AddDatabase(DatabaseKey.Primary, "fake", "second-secret");
        });
        using ServiceProvider provider = services.BuildServiceProvider();

        DatabaseDefinitionValidationException exception =
            Assert.Throws<DatabaseDefinitionValidationException>(
                () => provider.GetRequiredService<IDbRouter<DatabaseKey>>());

        Assert.DoesNotContain("first-secret", exception.ToString());
        Assert.DoesNotContain("second-secret", exception.ToString());
    }
}
