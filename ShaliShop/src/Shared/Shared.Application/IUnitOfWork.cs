namespace Shared.Application;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default);
    
}
 