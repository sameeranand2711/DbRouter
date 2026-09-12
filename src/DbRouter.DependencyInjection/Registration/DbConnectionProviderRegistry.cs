using System.Collections.Frozen;
using DbRouter.Core.Abstractions.Providers;
using DbRouter.Core.Exceptions;

namespace DbRouter.DependencyInjection.Registration;

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

            string providerId;

            try
            {
                providerId = provider.ProviderId;
            }
            catch (Exception)
            {
                throw new DbConnectionProviderRegistrationException(
                    "A database connection provider failed while reporting its provider identifier.");
            }

            if (string.IsNullOrWhiteSpace(providerId))
            {
                throw new DbConnectionProviderRegistrationException(
                    "A database connection provider has a missing provider identifier.");
            }

            if (!validated.TryAdd(providerId, provider))
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
