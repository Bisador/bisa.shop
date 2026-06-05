using InventoryService.Application.Inventories.Commands.Restock;
using InventoryService.Application.Inventories.Errors;

namespace InventoryService.Application.Test.Inventories.Commands;

public class InventoryRestockCommandHandlerTests
{
    private readonly Mock<IInventoryRepository> _inventories = new();
    private readonly Mock<IInventoryUnitOfWork> _unitOfWork = new();
    private readonly InventoryRestockCommandHandler _handler;

    public InventoryRestockCommandHandlerTests()
    {
        _handler = new InventoryRestockCommandHandler(_inventories.Object, _unitOfWork.Object);
    }
    [Fact]
    public async Task Should_fail_if_inventory_not_found()
    {
        var command = new InventoryRestockCommand(Guid.NewGuid(), Quantity: 5);

        _inventories.Setup(r => r.LoadAsync(command.InventoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Inventory?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InventoryNotFoundError>(); 
    }
    [Fact]
    public async Task Should_restock_inventory_and_commit()
    {
        var inventory = Inventory.Initialize(Guid.NewGuid(), quantity: 10); // Initial stock

        _inventories.Setup(r => r.LoadAsync(inventory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        var command = new InventoryRestockCommand(inventory.Id, Quantity: 5);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        inventory.QuantityOnHand.Should().Be(15);

        _inventories.Verify(r => r.SaveAsync(inventory, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task Should_raise_InventoryRestocked_event()
    {
        var inventory = Inventory.Initialize(Guid.NewGuid(), quantity: 30);
        var restockQty = 7;

        _inventories.Setup(r => r.LoadAsync(inventory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        await _handler.Handle(new InventoryRestockCommand(inventory.Id, restockQty), CancellationToken.None);

        inventory.DomainEvents.Any(e =>
            e is InventoryRestocked r &&
            r.AggregateId == inventory.Id &&
            r.ProductId == inventory.ProductId &&
            r.QuantityAdded == restockQty
        ).Should().BeTrue();
    }

}