using System.Collections.Frozen;
using DbRouter.EntityFrameworkCore.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DbRouter.EntityFrameworkCore.Providers;

internal sealed class DbContextOptionsConfiguratorRegistry<TContext>
    where TContext : DbContext
{
    private readonly FrozenDictionary<
        string,
        Action<DbContextOptionsBuilder<TContext>, string>> _configurators;

    public DbContextOptionsConfiguratorRegistry(
        IReadOnlyDictionary<
            string,
            Action<DbContextOptionsBuilder<TContext>, string>> configurators)
    {
        _configurators = configurators.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public Action<DbContextOptionsBuilder<TContext>, string> Resolve(string providerId) =>
        _configurators.TryGetValue(providerId, out Action<DbContextOptionsBuilder<TContext>, string>? configure)
            ? configure
            : throw new DbContextProviderNotFoundException();
}
