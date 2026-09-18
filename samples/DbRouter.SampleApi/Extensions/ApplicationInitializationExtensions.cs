using DbRouter.Core.Abstractions.Scoping;
using DbRouter.EntityFrameworkCore.Abstractions.Resolvers;
using DbRouter.SampleApi.Configuration;
using DbRouter.SampleApi.Data.Contexts;
using DbRouter.SampleApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DbRouter.SampleApi.Extensions;

public static class ApplicationInitializationExtensions
{
    public static async Task InitializeSampleDataAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await InitializeCustomersAsync(services, cancellationToken);
        await InitializeOrdersAsync(services, cancellationToken);
    }

    private static async Task InitializeCustomersAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IDatabaseSelection<DatabaseKey>>()
            .Select(DatabaseKey.Customer);
        CustomerDbContext context = scope.ServiceProvider
            .GetRequiredService<CustomerDbContext>();

        await context.Database.EnsureCreatedAsync(cancellationToken);

        if (!await context.Customers.AnyAsync(cancellationToken))
        {
            var customer = new Customer { Name = "DbRouter sample customer" };
            context.Customers.Add(customer);
            await context.SaveChangesAsync(cancellationToken);
            context.CustomerPreferences.Add(new CustomerPreference
            {
                CustomerId = customer.Id,
                Preference = "Demonstrate shared scoped DbContext",
            });
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task InitializeOrdersAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        IDbContextResolver<DatabaseKey, OrdersDbContext> resolver = scope.ServiceProvider
            .GetRequiredService<IDbContextResolver<DatabaseKey, OrdersDbContext>>();
        await using OrdersDbContext context = resolver.Create(DatabaseKey.Orders);

        await context.Database.EnsureCreatedAsync(cancellationToken);

        if (!await context.Orders.AnyAsync(cancellationToken))
        {
            context.Orders.Add(new Order { Description = "DbRouter sample order" });
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
