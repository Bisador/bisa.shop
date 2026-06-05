using Shared.Eventing;
using Shared.Eventing.Abstraction;
using Shared.Messaging;
using Shared.Messaging.Abstraction;
using Shared.Messaging.AzureServiceBusConfigurations;
using Shared.Messaging.RabbitMqConfigurations;
using Shared.Telemetry;

namespace WebApplication.Configurations;

public static class EventExtensions
{
    public static IServiceCollection RegisterEventHandling(this IServiceCollection services,
        IWebHostEnvironment environment, string connectionString)
    {
        // services.AddSingleton<IMessagePublisher>(sp => environment.IsDevelopment()
        //     ? RabbitMqPublisher.CreateAsync("localhost").Result
        //     : new AzureServiceBusPublisher(connectionString));

        services.AddSingleton<IDomainEventPublisher, MessageBusDomainEventPublisher>();
        // services.Decorate<IDomainEventPublisher, TelemetryDomainEventPublisherDecorator>();


        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }
}