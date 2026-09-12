using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using DbRouter.Core.Abstractions.Providers;
using DbRouter.Core.Abstractions.Resolvers;
using DbRouter.Core.Exceptions;
using DbRouter.Core.Models;
using DbRouter.Core.Validation;

namespace DbRouter.Core.Resolution;

/// <summary>
/// Resolves a validated, immutable snapshot of database definitions.
/// </summary>
public sealed class DbRouter<TKey> : IDbRouter<TKey>
    where TKey : notnull
{
    private readonly FrozenDictionary<TKey, DatabaseDefinition<TKey>> _definitions;

    /// <summary>Creates and validates a router from a static definition provider.</summary>
    public DbRouter(IDatabaseDefinitionProvider<TKey> definitionProvider)
    {
        ArgumentNullException.ThrowIfNull(definitionProvider);

        IReadOnlyCollection<DatabaseDefinition<TKey>> providedDefinitions;

        try
        {
            providedDefinitions = definitionProvider.GetDefinitions();
        }
        catch (Exception)
        {
            throw new DatabaseDefinitionValidationException(
                "The database definition provider failed to supply its static definitions.");
        }

        if (providedDefinitions is null)
        {
            throw new DatabaseDefinitionValidationException(
                "The database definition provider returned a null collection.");
        }

        DatabaseDefinition<TKey>[] definitions;

        try
        {
            definitions = providedDefinitions.ToArray();
        }
        catch (Exception)
        {
            throw new DatabaseDefinitionValidationException(
                "The database definition provider failed to supply its static definitions.");
        }

        var validated = new Dictionary<TKey, DatabaseDefinition<TKey>>();
        var index = 0;

        foreach (DatabaseDefinition<TKey>? definition in definitions)
        {
            DatabaseDefinitionValidator.Validate(definition, index);

            if (!validated.TryAdd(definition!.Key, definition))
            {
                throw new DatabaseDefinitionValidationException(
                    "The database definition set contains a duplicate key.");
            }

            index++;
        }

        _definitions = validated.ToFrozenDictionary();
    }

    /// <inheritdoc />
    public DatabaseDefinition<TKey> Resolve(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return _definitions.TryGetValue(key, out DatabaseDefinition<TKey>? definition)
            ? definition
            : throw new DatabaseNotFoundException();
    }

    /// <inheritdoc />
    public bool TryResolve(
        TKey key,
        [NotNullWhen(true)] out DatabaseDefinition<TKey>? definition)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _definitions.TryGetValue(key, out definition);
    }
}
