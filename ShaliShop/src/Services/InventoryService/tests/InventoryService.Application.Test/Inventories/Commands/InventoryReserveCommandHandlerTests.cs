using InventoryService.Application.Inventories.Commands.Reserve;
using InventoryService.Application.Inventories.Errors;

namespace InventoryService.Application.Test.Inventories.Commands;

public class InventoryReserveCommandHandlerTests
{
    private readonly Mock<IInventoryRepository> _inventories = new();
    private readonly Mock<IInventoryUnitOfWork> _unitOfWork = new();
    private readonly InventoryReserveCommandHandler _handler;

    public InventoryReserveCommandHandlerTests()
    {
        _handler = new InventoryReserveCommandHandler(_inventories.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_fail_if_inventory_not_found()
    {
        var command = new InventoryReserveCommand(Guid.NewGuid(), 5);

        _inventories.Setup(r => r.LoadAsync(command.InventoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Inventory?) null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InventoryNotFoundError>();
    }

    [Fact]
    public async Task Should_fail_if_not_enough_stock()
    {
        var inventory = Inventory.Initialize(Guid.NewGuid(), quantity: 10); // Available: 10
        inventory.Reserve(9); // Reserved: 9 → Available: 1

        _inventories.Setup(r => r.LoadAsync(inventory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        var command = new InventoryReserveCommand(inventory.Id, 2);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotEnoughAvailableInventoryToReserveException>();
    }

    [Fact]
    public async Task Should_reserve_inventory_and_commit()
    {
        var inventory = Inventory.Initialize(Guid.NewGuid(), quantity: 20);
        var command = new InventoryReserveCommand(inventory.Id, 5);

        _inventories.Setup(r => r.LoadAsync(inventory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        inventory.Reserved.Should().Be(5);

        _inventories.Verify(r => r.SaveAsync(inventory, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_raise_InventoryReserved_event()
    {
        var inventory = Inventory.Initialize(Guid.NewGuid(), quantity: 50);
        var command = new InventoryReserveCommand(inventory.Id, 10);

        _inventories.Setup(r => r.LoadAsync(inventory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        await _handler.Handle(command, CancellationToken.None);

        inventory.DomainEvents.Any(e =>
            e is InventoryReserved reserved &&
            reserved.AggregateId == inventory.Id &&
            reserved.ProductId == inventory.ProductId &&
            reserved.QuantityReserved == 10
        ).Should().BeTrue();
    }
}