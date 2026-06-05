using OrderService.Application.Orders.Commands.Errors;

namespace OrderService.Application.Orders.Commands.Return;

public class OrderReturnCommandHandler(
    IOrderRepository orders,
    IOrderUnitOfWork unitOfWork
) : IRequestHandler<OrderReturnCommand, Result>
{
    public async Task<Result> Handle(OrderReturnCommand command, CancellationToken ct)
    {
        var order = await orders.LoadAsync(command.OrderId, ct);
        if (order is null)
            return Result.Failure(new OrderNotFoundError(command.OrderId));

        var returnedItems = order.Items
            .Where(i => command.Items.Any(r => r.ProductId == i.ProductId))
            .ToList();

        if (returnedItems.Count == 0)
            return Result.Failure(new ReturnedItemNotFoundError());

        order.Return(returnedItems);

        await orders.SaveAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}