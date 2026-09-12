using DbRouter.EntityFrameworkCore.Exceptions;
using DbRouter.EntityFrameworkCore.Providers;
using Microsoft.EntityFrameworkCore;

namespace DbRouter.EntityFrameworkCore.Options;

/// <summary>Configures provider-specific EF Core options delegates without coupling DbRouter to a provider.</summary>
public sealed class DbRouterEntityFrameworkBuilder<TContext>
    where TContext : DbContext
{
    private readonly Dictionary<
        string,
        Action<DbContextOptionsBuilder<TContext>, string>> _configurators =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Adds an EF options configurator for one provider identifier.</summary>
    public DbRouterEntityFrameworkBuilder<TContext> AddProvider(
        string providerId,
        Action<DbContextOptionsBuilder<TContext>, string> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerId);
        ArgumentNullException.ThrowIfNull(configure);

        if (!_configurators.TryAdd(providerId, configure))
        {
            throw new DbContextProviderRegistrationException();
        }

        return this;
    }

    internal DbContextOptionsConfiguratorRegistry<TContext> Build() => new(_configurators);
}
