using System.Collections.Frozen;

namespace DbRouter.DependencyInjection;

internal sealed class DbConnectionProviderRegistry
{
    private readonly FrozenDictionary<string, IDbConnectionProvider> _providers;

    public DbConnectionProviderRegistry(IEnumerable<IDbConnectionProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        var validated = new Dictionary<string, IDbConnectionProvider>(StringComparer.OrdinalIgnoreCase);

        foreach (IDbConnectionProvider? provider in providers)
        {
            if (provider is null)
            {
                throw new DbConnectionProviderRegistrationException(
                    "A database connection provider registration resolved to null.");
            }

            if (string.IsNullOrWhiteSpace(provider.ProviderId))
            {
                throw new DbConnectionProviderRegistrationException(
                    "A database connection provider has a missing provider identifier.");
            }

            if (!validated.TryAdd(provider.ProviderId, provider))
            {
                throw new DbConnectionProviderRegistrationException(
                    "Multiple database connection providers use the same provider identifier.");
            }
        }

        _providers = validated.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public IDbConnectionProvider Resolve(string providerId) =>
        _providers.TryGetValue(providerId, out IDbConnectionProvider? provider)
            ? provider
            : throw new DbConnectionProviderNotFoundException();
}
