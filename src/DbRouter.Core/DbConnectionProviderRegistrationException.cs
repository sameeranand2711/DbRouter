namespace DbRouter;

/// <summary>Indicates an invalid or duplicate connection-provider registration.</summary>
public sealed class DbConnectionProviderRegistrationException : InvalidOperationException
{
    /// <summary>Creates an exception with a connection-string-safe message.</summary>
    public DbConnectionProviderRegistrationException(string message)
        : base(message)
    {
    }
}
