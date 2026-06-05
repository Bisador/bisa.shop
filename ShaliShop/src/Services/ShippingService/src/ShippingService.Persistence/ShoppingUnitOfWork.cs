using Shared.Eventing.Abstraction;
using Shared.Persistence;

namespace ShippingService.Persistence;

public class ShippingUnitOfWork(ShipmentDbContext dbContext, IDomainEventDispatcher dispatcher)
    : UnitOfWorkBase(dbContext, dispatcher);