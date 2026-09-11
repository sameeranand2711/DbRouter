namespace DbRouter.Core.Exceptions;

/// <summary>Indicates an attempt to change an established scoped selection.</summary>
public sealed class DatabaseSelectionConflictException : InvalidOperationException
{
    /// <summary>Creates the exception.</summary>
    public DatabaseSelectionConflictException()
        : base("A different database has already been selected for the current scope.")
    {
    }
}
