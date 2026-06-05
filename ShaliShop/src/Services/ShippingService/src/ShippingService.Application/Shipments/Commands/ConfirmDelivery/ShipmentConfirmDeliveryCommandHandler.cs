using ShippingService.Application.Shipments.Commands.Errors;

namespace ShippingService.Application.Shipments.Commands.ConfirmDelivery;

public class ShipmentConfirmDeliveryCommandHandler(
    IShipmentRepository shipments,
    IShippingUnitOfWork unitOfWork
) : IRequestHandler<ShipmentConfirmDeliveryCommand, Result>
{
    public async Task<Result> Handle(ShipmentConfirmDeliveryCommand command, CancellationToken ct)
    {
        var shipment = await shipments.LoadAsync(command.ShipmentId, ct);
        if (shipment is null)
            return Result.Failure(new ShipmentNotFoundError(command.ShipmentId));

        shipment.ConfirmDelivery();

        await shipments.SaveAsync(shipment, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}