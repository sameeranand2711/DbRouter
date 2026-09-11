namespace DbRouter.EntityFrameworkCore;

/// <summary>Indicates that no EF options configurator exists for a resolved provider.</summary>
public sealed class DbContextProviderNotFoundException : InvalidOperationException
{
    /// <summary>Creates a connection-string-safe exception.</summary>
    public DbContextProviderNotFoundException()
        : base("No Entity Framework Core options provider is registered for the resolved definition.")
    {
    }
}
