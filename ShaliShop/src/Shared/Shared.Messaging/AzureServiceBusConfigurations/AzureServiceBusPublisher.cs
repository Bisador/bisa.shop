using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Shared.Messaging.Abstraction;

namespace Shared.Messaging.AzureServiceBusConfigurations;

public class AzureServiceBusPublisher(string connectionString) : IMessagePublisher
{
    private readonly ServiceBusClient _client = new ServiceBusClient(connectionString);

    public async Task PublishAsync<T>(T message, string topic, CancellationToken cancellationToken = default)
    {
        var sender = _client.CreateSender(topic);
        var envelope = new MessageEnvelope<T>(message);
        var json = JsonSerializer.Serialize(envelope);
        var sbMessage = new ServiceBusMessage(json);
        await sender.SendMessageAsync(sbMessage);
    }
}