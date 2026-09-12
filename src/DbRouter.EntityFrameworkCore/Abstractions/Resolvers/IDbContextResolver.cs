using Microsoft.EntityFrameworkCore;

namespace DbRouter.EntityFrameworkCore.Abstractions.Resolvers;

/// <summary>Creates caller-owned EF Core contexts for explicit database keys.</summary>
public interface IDbContextResolver<TKey, TContext>
    where TKey : notnull
    where TContext : DbContext
{
    /// <summary>Creates a distinct context configured for the requested key.</summary>
    TContext Create(TKey key);
}
