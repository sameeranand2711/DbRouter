namespace DbRouter;

/// <summary>Indicates that no connection provider is registered for a definition.</summary>
public sealed class DbConnectionProviderNotFoundException : InvalidOperationException
{
    /// <summary>Creates the exception without exposing configuration values.</summary>
    public DbConnectionProviderNotFoundException()
        : base("No database connection provider is registered for the resolved definition.")
    {
    }
}
