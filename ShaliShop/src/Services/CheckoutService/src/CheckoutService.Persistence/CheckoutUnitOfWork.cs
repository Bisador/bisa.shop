 
using Shared.Eventing.Abstraction;
using Shared.Persistence;

namespace CheckoutService.Persistence;

public sealed class CheckoutUnitOfWork(CheckoutDbContext dbContext, IDomainEventDispatcher dispatcher)
    : UnitOfWorkBase(dbContext, dispatcher);