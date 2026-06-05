using Microsoft.EntityFrameworkCore;
using Shared.Application;
using Shared.Domain;
using Shared.Eventing.Abstraction;

namespace Shared.Persistence;

public abstract class UnitOfWorkBase(DbContext dbContext, IDomainEventDispatcher dispatcher) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        await DispatchDomainEventsAsync(cancellationToken);
    }

    public async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        await dispatcher.DispatchAsync(aggregates, cancellationToken);
    }
}