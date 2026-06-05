using InventoryService.Application.Inventories.Errors;

namespace InventoryService.Application.Inventories.Commands.SetLowStockThreshold;

public class InventorySetLowStockThresholdCommandHandler(
    IInventoryRepository inventories,
    IInventoryUnitOfWork unitOfWork
) : IRequestHandler<InventorySetLowStockThresholdCommand, Result>
{
    public async Task<Result> Handle(InventorySetLowStockThresholdCommand command, CancellationToken ct)
    {
        var inventory = await inventories.LoadAsync(command.InventoryId, ct);
        if (inventory is null)
            return Result.Failure(new InventoryNotFoundError(command.InventoryId));   

        inventory.SetLowStockThreshold(command.Threshold);

        await inventories.SaveAsync(inventory, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}