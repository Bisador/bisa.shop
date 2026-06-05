using OrderService.Application.Abstraction;
using OrderService.Application.Orders.Commands.OrderPay;
using OrderService.Application.Tests.TestUtils;
using OrderService.Domain.Orders.Aggregates;
using OrderService.Domain.Orders.DomainEvents;
using OrderService.Domain.Orders.Enums;
using OrderService.Domain.Orders.Repository;

namespace OrderService.Application.Tests.Orders.Commands;

public class OrderPayCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IOrderUnitOfWork> _unitOfWork = new();
    private readonly OrderPayCommandHandler _handler;

    public OrderPayCommandHandlerTests()
    {
        _handler = new OrderPayCommandHandler(_orders.Object, _unitOfWork.Object);
    }
    
    [Fact]
    public async Task Should_fail_when_order_is_not_found()
    {
        var command = new OrderPayCommand(Guid.NewGuid(), FakePaymentDto.Valid());
    
        _orders.Setup(r => r.LoadAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }
    
    [Fact]
    public async Task Should_fail_when_payment_method_is_invalid()
    {
        var order = FakeOrder.Placed(out var orderId);
        var dto = new PaymentDto("tx123", "INVALID_METHOD", DateTime.UtcNow);

        _orders.Setup(r => r.LoadAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new OrderPayCommand(orderId, dto), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0); 
    }
    
    [Fact]
    public async Task Should_mark_order_as_paid_and_commit()
    {
        var order = FakeOrder.Placed(out var orderId);
        var dto = new PaymentDto("tx123", "CreditCard", DateTime.UtcNow);

        _orders.Setup(r => r.LoadAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new OrderPayCommand(orderId, dto), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid);

        _orders.Verify(r => r.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

   
    [Fact]
    public async Task Should_raise_OrderPaid_event_when_payment_is_successful()
    {
        // Arrange
        var order = FakeOrder.Placed(out var orderId);
        var dto = new PaymentDto("TX-ORDER-999", nameof(PaymentMethod.CreditCard), DateTime.UtcNow);

        var handler = new OrderPayCommandHandler(
            orders: Mock.Of<IOrderRepository>(r =>
                r.LoadAsync(orderId, It.IsAny<CancellationToken>()) == Task.FromResult(order)
            ),
            unitOfWork: Mock.Of<IOrderUnitOfWork>()
        );

        // Act
        var result = await handler.Handle(new OrderPayCommand(orderId, dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
 
        order.DomainEvents.Any(e =>
            e is OrderPaid paidEvent &&
            paidEvent.AggregateId == order.Id &&
            paidEvent.TransactionId == "TX-ORDER-999")
            .Should().BeTrue();
    }

}