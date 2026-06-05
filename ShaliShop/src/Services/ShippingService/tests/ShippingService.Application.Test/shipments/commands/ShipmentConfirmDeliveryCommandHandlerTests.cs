using ShippingService.Application.Shipments.Commands.ConfirmDelivery;
using ShippingService.Application.Shipments.Commands.Errors;
using ShippingService.Domain.Shipments.Enums;
using ShippingService.Domain.Shipments.Exceptions;

namespace ShippingService.Application.Test.shipments.commands;

public class ShipmentConfirmDeliveryCommandHandlerTests
{
    private readonly Mock<IShipmentRepository> _shipments = new();
    private readonly Mock<IShippingUnitOfWork> _unitOfWork = new();
    private readonly ShipmentConfirmDeliveryCommandHandler _handler;

    public ShipmentConfirmDeliveryCommandHandlerTests()
    {
        _handler = new ShipmentConfirmDeliveryCommandHandler(_shipments.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_fail_if_shipment_not_found()
    {
        var command = new ShipmentConfirmDeliveryCommand(Guid.NewGuid());

        _shipments.Setup(r => r.LoadAsync(command.ShipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shipment?) null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ShipmentNotFoundError>();
    }

    [Fact]
    public async Task Should_fail_if_shipment_not_dispatched()
    {
        var shipment = Shipment.Create(Guid.NewGuid());

        _shipments.Setup(r => r.LoadAsync(shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        var command = new ShipmentConfirmDeliveryCommand(shipment.Id);
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ShipmentMustBeDispatchedBeforeDeliveryException>();
    }

    [Fact]
    public async Task Should_confirm_delivery_and_commit()
    {
        var shipment = Shipment.Create(Guid.NewGuid());
        shipment.Dispatch("FedEx", "FX456");

        _shipments.Setup(r => r.LoadAsync(shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        var result = await _handler.Handle(new ShipmentConfirmDeliveryCommand(shipment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        shipment.Status.Should().Be(ShipmentStatus.Delivered);

        _shipments.Verify(r => r.SaveAsync(shipment, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}