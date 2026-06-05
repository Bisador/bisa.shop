using ShippingService.Application.Shipments.Commands.Dispatch;
using ShippingService.Application.Shipments.Commands.Errors;
using ShippingService.Domain.Shipments.DomainEvents;
using ShippingService.Domain.Shipments.Enums;
using ShippingService.Domain.Shipments.Exceptions;

namespace ShippingService.Application.Test.shipments.commands;

public class ShipmentDispatchCommandHandlerTests
{
    private readonly Mock<IShipmentRepository> _shipments = new();
    private readonly Mock<IShippingUnitOfWork> _unitOfWork = new();
    private readonly ShipmentDispatchCommandHandler _handler;

    public ShipmentDispatchCommandHandlerTests()
    {
        _handler = new ShipmentDispatchCommandHandler(_shipments.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_fail_if_shipment_not_found()
    {
        var command = new ShipmentDispatchCommand(Guid.NewGuid(), "DHL", "TRACK123");

        _shipments.Setup(r => r.LoadAsync(command.ShipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shipment?) null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ShipmentNotFoundError>();
    }

    [Fact]
    public async Task Should_fail_if_shipment_not_created()
    {
        var shipment = Shipment.Create(Guid.NewGuid());
        shipment.Dispatch("UPS", "TRACK000"); // now in Dispatched state

        _shipments.Setup(r => r.LoadAsync(shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        var command = new ShipmentDispatchCommand(shipment.Id, "DHL", "TRACK123");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<OnlyNewlyCreatedShipmentsCanBeDispatchedException>();
    }

    [Fact]
    public async Task Should_dispatch_shipment_and_commit()
    {
        var shipment = Shipment.Create(Guid.NewGuid());

        _shipments.Setup(r => r.LoadAsync(shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        var command = new ShipmentDispatchCommand(shipment.Id, "FedEx", "FX123456");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        shipment.Status.Should().Be(ShipmentStatus.Dispatched);
        shipment.Carrier.Should().Be("FedEx");
        shipment.TrackingNumber.Should().Be("FX123456");

        _shipments.Verify(r => r.SaveAsync(shipment, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_raise_ShipmentDispatched_event()
    {
        var shipment = Shipment.Create(Guid.NewGuid());
        var command = new ShipmentDispatchCommand(shipment.Id, "FedEx", "FX123456");

        _shipments.Setup(r => r.LoadAsync(shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        await _handler.Handle(command, CancellationToken.None);

        shipment.DomainEvents.Any(e =>
            e is ShipmentDispatched dispatched &&
            dispatched.AggregateId == shipment.Id &&
            dispatched.DispatchedAt != default
        ).Should().BeTrue();
    }
}