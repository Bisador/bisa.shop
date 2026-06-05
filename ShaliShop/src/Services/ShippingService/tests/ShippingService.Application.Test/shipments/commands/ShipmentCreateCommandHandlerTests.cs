using ShippingService.Application.Shipments.Commands.Create;
using ShippingService.Application.Shipments.Commands.Errors;
using ShippingService.Domain.Shipments.DomainEvents;

namespace ShippingService.Application.Test.shipments.commands;

public class ShipmentCreateCommandHandlerTests
{
    private readonly Mock<IShipmentRepository> _shipments = new();
    private readonly Mock<IShippingUnitOfWork> _unitOfWork = new();
    private readonly CreateShipmentCommandHandler _handler;

    public ShipmentCreateCommandHandlerTests()
    {
        _handler = new CreateShipmentCommandHandler(_shipments.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_create_shipment_and_return_id()
    {
        var orderId = Guid.NewGuid();
        var command = new ShipmentCreateCommand(orderId);

        _shipments.Setup(r => r.FindByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shipment?) null);

        _shipments.Setup(r => r.SaveAsync(It.IsAny<Shipment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _shipments.Verify(r => r.SaveAsync(It.Is<Shipment>(s => s.OrderId == orderId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_fail_if_shipment_already_exists()
    {
        var orderId = Guid.NewGuid();
        var existing = Shipment.Create(orderId);

        _shipments.Setup(r => r.FindByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(new ShipmentCreateCommand(orderId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<ShipmentAlreadyExistsError>();
    }

    [Fact]
    public async Task Should_raise_ShipmentCreated_event()
    {
        var orderId = Guid.NewGuid();
        var command = new ShipmentCreateCommand(orderId);

        _shipments.Setup(r => r.FindByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shipment?) null);

        Shipment? createdShipment = null;
        _shipments.Setup(r => r.SaveAsync(It.IsAny<Shipment>(), It.IsAny<CancellationToken>()))
            .Callback<Shipment, CancellationToken>((s, _) => createdShipment = s)
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        createdShipment.Should().NotBeNull();
        createdShipment!.DomainEvents.Any(e =>
            e is ShipmentCreated created &&
            created.OrderId == orderId
        ).Should().BeTrue();
    }
}