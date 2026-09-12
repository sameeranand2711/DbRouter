using DbRouter.SampleApi.Data.Contexts;
using DbRouter.SampleApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DbRouter.SampleApi.Data.Repositories;

public sealed class CustomerPreferenceRepository
{
    private readonly CustomerDbContext _dbContext;

    public CustomerPreferenceRepository(CustomerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(CustomerPreference preference) =>
        _dbContext.CustomerPreferences.Add(preference);

    public Task<int> CountAsync(CancellationToken cancellationToken) =>
        _dbContext.CustomerPreferences.CountAsync(cancellationToken);
}
