using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbRouter.EntityFrameworkCore.Tests;

public sealed class EfCoreRegistrationTests
{
    private const string Secret = "database-name-containing-secret";

    [Fact]
    public void Duplicate_provider_configurators_are_rejected_case_insensitively()
    {
        var services = new ServiceCollection();

        Assert.Throws<DbContextProviderRegistrationException>(
            () => services.AddDbRouterEntityFrameworkCore<DatabaseKey, TestDbContext>(
                options => new TestDbContext(options),
                builder =>
                {
                    builder.AddProvider("provider", (options, name) => options.UseInMemoryDatabase(name));
                    builder.AddProvider("PROVIDER", (options, name) => options.UseInMemoryDatabase(name));
                }));
    }

    [Fact]
    public void Missing_ef_provider_fails_without_leaking_connection_string()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
            builder.AddDatabase(DatabaseKey.Primary, "missing", Secret));
        services.AddDbRouterEntityFrameworkCore<DatabaseKey, TestDbContext>(
            options => new TestDbContext(options),
            _ => { });
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        IDbContextResolver<DatabaseKey, TestDbContext> resolver = scope.ServiceProvider
            .GetRequiredService<IDbContextResolver<DatabaseKey, TestDbContext>>();

        DbContextProviderNotFoundException exception =
            Assert.Throws<DbContextProviderNotFoundException>(
                () => resolver.Create(DatabaseKey.Primary));

        Assert.DoesNotContain(Secret, exception.ToString());
    }

    [Fact]
    public void Configurator_failure_is_sanitized()
    {
        var services = new ServiceCollection();
        services.AddDbRouter<DatabaseKey>(builder =>
            builder.AddDatabase(DatabaseKey.Primary, "throwing", Secret));
        services.AddDbRouterEntityFrameworkCore<DatabaseKey, TestDbContext>(
            options => new TestDbContext(options),
            builder => builder.AddProvider(
                "throwing",
                (_, connectionString) =>
                    throw new InvalidOperationException($"Failure for {connectionString}")));
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        IDbContextResolver<DatabaseKey, TestDbContext> resolver = scope.ServiceProvider
            .GetRequiredService<IDbContextResolver<DatabaseKey, TestDbContext>>();

        DbContextCreationException exception = Assert.Throws<DbContextCreationException>(
            () => resolver.Create(DatabaseKey.Primary));

        Assert.DoesNotContain(Secret, exception.ToString());
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Ef_integration_has_no_concrete_database_provider_reference()
    {
        string[] references = typeof(IDbContextResolver<,>)
            .Assembly
            .GetReferencedAssemblies()
            .Select(name => name.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain("Microsoft.Data.SqlClient", references);
        Assert.DoesNotContain("Npgsql", references);
    }

    [Fact]
    public void Non_ef_packages_have_no_ef_core_reference()
    {
        string[][] references =
        [
            typeof(IDbRouter<>).Assembly.GetReferencedAssemblies().Select(name => name.Name ?? string.Empty).ToArray(),
            typeof(DbRouterBuilder<>).Assembly.GetReferencedAssemblies().Select(name => name.Name ?? string.Empty).ToArray(),
        ];

        Assert.All(
            references,
            packageReferences => Assert.DoesNotContain(
                packageReferences,
                name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)));
    }
}
