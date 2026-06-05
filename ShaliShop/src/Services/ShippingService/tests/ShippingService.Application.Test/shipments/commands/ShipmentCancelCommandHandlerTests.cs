using ShippingService.Application.Shipments.Commands.Cancel;
using ShippingService.Domain.Shipments.Enums;
using ShippingService.Domain.Shipments.Exceptions;

namespace ShippingService.Application.Test.shipments.commands;

public class ShipmentCancelCommandHandlerTests
{
    private readonly Mock<IShipmentRepository> _shipments = new();
    private readonly Mock<IShippingUnitOfWork> _unitOfWork = new();
    private readonly ShipmentCancelCommandHandler _handler;

    public ShipmentCancelCommandHandlerTests()
    {
        _handler = new ShipmentCancelCommandHandler(_shipments.Object, _unitOfWork.Object);
    }
    
    [Fact]
    public async Task Should_fail_if_already_delivered()
    {
        var shipment = Shipment.Create(Guid.NewGuid());
        shipment.Dispatch("DHL", "DX999");
        shipment.ConfirmDelivery(); // now Delivered

        _shipments.Setup(r => r.LoadAsync(shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        var command = new ShipmentCancelCommand(shipment.Id);
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CannotCancelDeliveredShipmentException>();
    }

    [Fact]
    public async Task Should_cancel_shipment_and_commit()
    {
        var shipment = Shipment.Create(Guid.NewGuid());

        _shipments.Setup(r => r.LoadAsync(shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        var command = new ShipmentCancelCommand(shipment.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        shipment.Status.Should().Be(ShipmentStatus.Canceled);

        _shipments.Verify(r => r.SaveAsync(shipment, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

}