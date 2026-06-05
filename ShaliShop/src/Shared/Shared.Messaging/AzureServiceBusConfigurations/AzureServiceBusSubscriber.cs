using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Shared.Eventing.Abstraction;

namespace Shared.Messaging.AzureServiceBusConfigurations;

public class AzureServiceBusSubscriber(string connectionString) : IMessageSubscriber
{
    private readonly ServiceBusClient _client = new ServiceBusClient(connectionString);

    public async Task SubscribeAsync<T>(string topic, Func<T, Task> handler)
    {
        var processor = _client.CreateProcessor(topic, new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += async args =>
        {
            var json = args.Message.Body.ToString();
            var message = JsonSerializer.Deserialize<T>(json);
            await handler(message);
        };

        processor.ProcessErrorAsync += args =>
        {
            Console.WriteLine($"❌ Error: {args.Exception.Message}");
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync();
    }
}