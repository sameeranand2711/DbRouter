namespace DbRouter;

/// <summary>Indicates that no definition exists for a requested key.</summary>
public sealed class DatabaseNotFoundException : KeyNotFoundException
{
    /// <summary>Creates an exception without stringifying the requested key.</summary>
    public DatabaseNotFoundException()
        : base("No database definition is registered for the requested key.")
    {
    }
}
