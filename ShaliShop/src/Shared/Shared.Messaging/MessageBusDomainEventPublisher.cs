using Shared.Domain;
using Shared.Eventing.Abstraction;
using Shared.Messaging.Abstraction;

namespace Shared.Messaging;

public class MessageBusDomainEventPublisher(IMessagePublisher? messagePublisher = null) : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var topic = domainEvent.GetType().Name;
        if (messagePublisher is null)
            return Task.CompletedTask;
        return messagePublisher.PublishAsync(domainEvent, topic, cancellationToken);
    }
}