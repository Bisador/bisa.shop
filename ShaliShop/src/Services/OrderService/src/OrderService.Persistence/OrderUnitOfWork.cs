using Shared.Eventing.Abstraction;
using Shared.Persistence;

namespace OrderService.Persistence;

public class OrderUnitOfWork(OrderDbContext dbContext, IDomainEventDispatcher dispatcher)
    : UnitOfWorkBase(dbContext, dispatcher);