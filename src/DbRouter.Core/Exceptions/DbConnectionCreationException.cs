namespace DbRouter.Core.Exceptions;

/// <summary>Indicates that a provider failed to construct a valid closed connection.</summary>
public sealed class DbConnectionCreationException : InvalidOperationException
{
    /// <summary>
    /// Creates a sanitized exception. Provider exceptions are deliberately not retained because
    /// third-party exception messages can echo connection strings.
    /// </summary>
    public DbConnectionCreationException()
        : base("The database connection provider failed to create a valid closed connection.")
    {
    }
}
