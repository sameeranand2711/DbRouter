using System.Collections;

namespace DbRouter.Core.Tests;

internal sealed class ThrowingDefinitionCollection :
    IReadOnlyCollection<DatabaseDefinition<DatabaseKey>>
{
    private readonly string _secret;

    public ThrowingDefinitionCollection(string secret)
    {
        _secret = secret;
    }

    public int Count => 1;

    public IEnumerator<DatabaseDefinition<DatabaseKey>> GetEnumerator() =>
        throw new InvalidOperationException($"Enumeration failure containing {_secret}");

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
