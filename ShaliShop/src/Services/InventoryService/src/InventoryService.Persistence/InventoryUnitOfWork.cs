using Shared.Eventing;
using Shared.Eventing.Abstraction;
using Shared.Persistence;

namespace InventoryService.Persistence;

public sealed class InventoryUnitOfWork(InventoryDbContext dbContext, IDomainEventDispatcher dispatcher)
    : UnitOfWorkBase(dbContext, dispatcher);