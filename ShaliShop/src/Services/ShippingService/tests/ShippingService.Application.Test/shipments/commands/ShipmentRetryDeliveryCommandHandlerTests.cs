using ShippingService.Application.Shipments.Commands.Errors;
using ShippingService.Application.Shipments.Commands.RetryDelivery;
using ShippingService.Domain.Shipments.DomainEvents;
using ShippingService.Domain.Shipments.Exceptions;

namespace ShippingService.Application.Test.shipments.commands;

public class ShipmentRetryDeliveryCommandHandlerTests
{
    private readonly Mock<IShipmentRepository> _shipments = new();
    private readonly Mock<IShippingUnitOfWork> _unitOfWork = new();
    private readonly ShipmentRetryDeliveryCommandHandler _handler;

    public ShipmentRetryDeliveryCommandHandlerTests()
    {
        _handler = new ShipmentRetryDeliveryCommandHandler(_shipments.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_fail_if_shipment_not_found()
    {
        var command = new ShipmentRetryDeliveryCommand(Guid.NewGuid());

        _shipments.Setup(r => r.LoadAsync(command.ShipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shipment?) null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ShipmentNotFoundError>();
    }

    [Fact]
    public async Task Should_fail_if_shipment_not_dispatched()
    {
        var shipment = Shipment.Create(Guid.NewGuid()); // Status: Created

        _shipments.Setup(r => r.LoadAsync(shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        var command = new ShipmentRetryDeliveryCommand(shipment.Id);
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RetryOnlyAllowedForDispatchedShipmentsException>();
    }

    [Fact]
    public async Task Should_retry_delivery_and_commit()
    {
        var shipment = Shipment.Create(Guid.NewGuid());
        shipment.Dispatch("UPS", "TRACK123");

        _shipments.Setup(r => r.LoadAsync(shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        var command = new ShipmentRetryDeliveryCommand(shipment.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _shipments.Verify(r => r.SaveAsync(shipment, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_raise_ShipmentDeliveryRetried_event()
    {
        var shipment = Shipment.Create(Guid.NewGuid());
        shipment.Dispatch("UPS", "TRACK123");

        _shipments.Setup(r => r.LoadAsync(shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        await _handler.Handle(new ShipmentRetryDeliveryCommand(shipment.Id), CancellationToken.None);

        shipment.DomainEvents.Any(e =>
            e is ShipmentDeliveryRetried retried &&
            retried.AggregateId == shipment.Id &&
            retried.RetriedAt != default
        ).Should().BeTrue();
    }
}