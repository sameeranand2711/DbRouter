using DbRouter.SampleApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DbRouter.SampleApi.Data.Contexts;

public sealed class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<CustomerPreference> CustomerPreferences => Set<CustomerPreference>();
}
