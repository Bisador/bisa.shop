using InventoryService.Application.Inventories.Commands.SetLowStockThreshold;
using InventoryService.Application.Inventories.Errors;

namespace InventoryService.Application.Test.Inventories.Commands;

public class InventorySetLowStockThresholdCommandHandlerTests
{
    private readonly Mock<IInventoryRepository> _inventories = new();
    private readonly Mock<IInventoryUnitOfWork> _unitOfWork = new();
    private readonly InventorySetLowStockThresholdCommandHandler _handler;

    public InventorySetLowStockThresholdCommandHandlerTests()
    {
        _handler = new InventorySetLowStockThresholdCommandHandler(_inventories.Object, _unitOfWork.Object);
    }
    
    [Fact]
    public async Task Should_fail_if_inventory_not_found()
    {
        var command = new InventorySetLowStockThresholdCommand(Guid.NewGuid(), Threshold: 10);

        _inventories.Setup(r => r.LoadAsync(command.InventoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Inventory?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InventoryNotFoundError>(); 
    }
    [Fact]
    public async Task Should_set_threshold_and_commit()
    {
        var inventory = Inventory.Initialize(Guid.NewGuid(), quantity: 50);
        var command = new InventorySetLowStockThresholdCommand(inventory.Id, Threshold: 5);

        _inventories.Setup(r => r.LoadAsync(inventory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _inventories.Verify(r => r.SaveAsync(inventory, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

}