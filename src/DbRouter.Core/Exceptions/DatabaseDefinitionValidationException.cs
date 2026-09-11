namespace DbRouter.Core.Exceptions;

/// <summary>Indicates that a database definition snapshot is invalid.</summary>
public sealed class DatabaseDefinitionValidationException : InvalidOperationException
{
    /// <summary>Creates an exception with a connection-string-safe message.</summary>
    public DatabaseDefinitionValidationException(string message)
        : base(message)
    {
    }
}
