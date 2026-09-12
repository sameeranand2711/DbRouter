using DbRouter.SampleApi.Data.Contexts;
using DbRouter.SampleApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DbRouter.SampleApi.Data.Repositories;

public sealed class CustomerRepository
{
    private readonly CustomerDbContext _dbContext;

    public CustomerRepository(CustomerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Customer customer) => _dbContext.Customers.Add(customer);

    public Task<int> CountAsync(CancellationToken cancellationToken) =>
        _dbContext.Customers.CountAsync(cancellationToken);
}
