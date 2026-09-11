namespace DbRouter;

/// <summary>Indicates that a scoped database has not been selected.</summary>
public sealed class DatabaseSelectionMissingException : InvalidOperationException
{
    /// <summary>Creates the exception.</summary>
    public DatabaseSelectionMissingException()
        : base("No database has been selected for the current scope.")
    {
    }
}
