using OrderService.Application.Orders.Commands.Errors;
using OrderService.Domain.Orders.Enums;

namespace OrderService.Application.Orders.Commands.OrderPay;

public class OrderPayCommandHandler(
    IOrderRepository orders,
    IOrderUnitOfWork unitOfWork) : IRequestHandler<OrderPayCommand, Result>
{
    public async Task<Result> Handle(OrderPayCommand command, CancellationToken ct)
    {
        var order = await orders.LoadAsync(command.OrderId, ct);
        if (order is null)
            return Result.Failure(new OrderNotFoundError(command.OrderId));

        var methodParsed = Enum.TryParse<PaymentMethod>(command.Payment.Method, out var method);
        if (!methodParsed)
            return Result.Failure(new InvalidPaymentMethodError());

        var payment = new Payment(
            command.Payment.TransactionId,
            method,
            command.Payment.PaidAt
        );
        order.Pay(payment);

        await orders.SaveAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}