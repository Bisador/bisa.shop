
using ShippingService.Application.Shipments.Commands.Errors;

namespace ShippingService.Application.Shipments.Commands.Cancel;

public class ShipmentCancelCommandHandler(
    IShipmentRepository shipments,
    IShippingUnitOfWork unitOfWork
) : IRequestHandler<ShipmentCancelCommand, Result>
{
    public async Task<Result> Handle(ShipmentCancelCommand command, CancellationToken ct)
    {
        var shipment = await shipments.LoadAsync(command.ShipmentId, ct);
        if (shipment is null)
            return Result.Failure(new ShipmentNotFoundError(command.ShipmentId)); 

        shipment.Cancel();

        await shipments.SaveAsync(shipment, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}