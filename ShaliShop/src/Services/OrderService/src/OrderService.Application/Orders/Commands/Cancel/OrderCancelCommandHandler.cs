 
using OrderService.Application.Orders.Commands.Errors;

namespace OrderService.Application.Orders.Commands.Cancel;

public class OrderCancelCommandHandler(
    IOrderRepository orders,
    IOrderUnitOfWork unitOfWork
) : IRequestHandler<OrderCancelCommand, Result>
{
    public async Task<Result> Handle(OrderCancelCommand command, CancellationToken ct)
    {
        var order = await orders.LoadAsync(command.OrderId, ct);
        if (order is null)
            return Result.Failure(new OrderNotFoundError(command.OrderId));

        order.Cancel(command.Reason);

        await orders.SaveAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}