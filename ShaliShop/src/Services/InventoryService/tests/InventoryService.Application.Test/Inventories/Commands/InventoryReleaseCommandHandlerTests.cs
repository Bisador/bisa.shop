using InventoryService.Application.Inventories.Commands.Release;
using InventoryService.Application.Inventories.Errors;

namespace InventoryService.Application.Test.Inventories.Commands;

public class InventoryReleaseCommandHandlerTests
{
    private readonly Mock<IInventoryRepository> _inventories = new();
    private readonly Mock<IInventoryUnitOfWork> _unitOfWork = new();
    private readonly InventoryReleaseCommandHandler _handler;

    public InventoryReleaseCommandHandlerTests()
    {
        _handler = new InventoryReleaseCommandHandler(_inventories.Object, _unitOfWork.Object);
    }
    
    [Fact]
    public async Task Should_fail_if_inventory_not_found()
    {
        var command = new InventoryReleaseCommand(Guid.NewGuid(), Quantity: 3);

        _inventories.Setup(r => r.LoadAsync(command.InventoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Inventory?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InventoryNotFoundError>();
    }
    [Fact]
    public async Task Should_fail_if_quantity_exceeds_reserved()
    {
        var inventory = Inventory.Initialize(Guid.NewGuid(), quantity: 10);
        inventory.Reserve(5); // Reserved: 5

        _inventories.Setup(r => r.LoadAsync(inventory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        var command = new InventoryReleaseCommand(inventory.Id, Quantity: 6);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidReleaseQuantityException>();
    }
    [Fact]
    public async Task Should_release_inventory_and_commit()
    {
        var inventory = Inventory.Initialize(Guid.NewGuid(), quantity: 20);
        inventory.Reserve(8); // Reserved: 8

        _inventories.Setup(r => r.LoadAsync(inventory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        var command = new InventoryReleaseCommand(inventory.Id, Quantity: 3);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        inventory.Reserved.Should().Be(5);

        _inventories.Verify(r => r.SaveAsync(inventory, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task Should_raise_InventoryReleased_event()
    {
        var inventory = Inventory.Initialize(Guid.NewGuid(), quantity: 30);
        inventory.Reserve(10);

        _inventories.Setup(r => r.LoadAsync(inventory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        var releaseQuantity = 4;
        await _handler.Handle(new InventoryReleaseCommand(inventory.Id, releaseQuantity), CancellationToken.None);

        inventory.DomainEvents.Any(e =>
            e is InventoryReleased released &&
            released.AggregateId == inventory.Id &&
            released.ProductId == inventory.ProductId &&
            released.QuantityReleased == releaseQuantity
        ).Should().BeTrue();
    }

}