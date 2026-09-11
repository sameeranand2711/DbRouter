namespace DbRouter.EntityFrameworkCore.Exceptions;

/// <summary>Indicates duplicate EF options-provider registration.</summary>
public sealed class DbContextProviderRegistrationException : InvalidOperationException
{
    /// <summary>Creates the exception.</summary>
    public DbContextProviderRegistrationException()
        : base("Multiple Entity Framework Core options providers use the same provider identifier.")
    {
    }
}
