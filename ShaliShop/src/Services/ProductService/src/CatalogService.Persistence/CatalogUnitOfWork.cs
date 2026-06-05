 
using Shared.Eventing.Abstraction;
using Shared.Persistence;

namespace CatalogService.Persistence;

public sealed class CatalogUnitOfWork(CatalogDbContext dbContext, IDomainEventDispatcher dispatcher)
    : UnitOfWorkBase(dbContext, dispatcher), ICatalogUnitOfWork;