using DbRouter.SampleApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DbRouter.SampleApi.Data.Contexts;

public sealed class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
}
