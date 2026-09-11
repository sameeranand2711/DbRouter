namespace DbRouter.EntityFrameworkCore;

/// <summary>Indicates sanitized EF options configuration or context activation failure.</summary>
public sealed class DbContextCreationException : InvalidOperationException
{
    /// <summary>Creates the exception without retaining potentially secret-bearing details.</summary>
    public DbContextCreationException()
        : base("Entity Framework Core context creation failed.")
    {
    }
}
