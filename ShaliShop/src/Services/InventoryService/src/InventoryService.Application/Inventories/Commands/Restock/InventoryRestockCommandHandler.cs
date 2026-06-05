using InventoryService.Application.Inventories.Errors;

namespace InventoryService.Application.Inventories.Commands.Restock;

public class InventoryRestockCommandHandler(
    IInventoryRepository inventories,
    IInventoryUnitOfWork unitOfWork
) : IRequestHandler<InventoryRestockCommand, Result>
{
    public async Task<Result> Handle(InventoryRestockCommand command, CancellationToken ct)
    {
        var inventory = await inventories.LoadAsync(command.InventoryId, ct);
        if (inventory is null)
            return Result.Failure(new InventoryNotFoundError(command.InventoryId));

        inventory.Restock(command.Quantity);

        await inventories.SaveAsync(inventory, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}