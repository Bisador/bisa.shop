using OrderService.Application.Abstraction;
using OrderService.Application.Orders.Commands.Confirm;
using OrderService.Application.Orders.Commands.Errors;
using OrderService.Application.Tests.TestUtils;
using OrderService.Domain.Orders.Aggregates;
using OrderService.Domain.Orders.DomainEvents;
using OrderService.Domain.Orders.Enums;
using OrderService.Domain.Orders.Exceptions;
using OrderService.Domain.Orders.Repository;

namespace OrderService.Application.Tests.Orders.Commands;

public class OrderConfirmCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IOrderUnitOfWork> _unitOfWork = new();
    private readonly OrderConfirmCommandHandler _handler;

    public OrderConfirmCommandHandlerTests()
    {
        _handler = new OrderConfirmCommandHandler(_orders.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_fail_if_order_not_found()
    {
        var command = new OrderConfirmCommand(Guid.NewGuid());

        _orders.Setup(r => r.LoadAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?) null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<OrderNotFoundError>();
    }

    [Fact]
    public async Task Should_fail_if_order_is_not_paid()
    {
        var order = FakeOrder.Placed(out var orderId); // Status: Placed

        _orders.Setup(r => r.LoadAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new OrderConfirmCommand(orderId);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<OnlyPaidOrdersCanBeConfirmedException>();
    }

    [Fact]
    public async Task Should_confirm_paid_order_and_commit()
    {
        var order = FakeOrder.Paid(out var orderId); // Status: Paid

        _orders.Setup(r => r.LoadAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new OrderConfirmCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);

        _orders.Verify(r => r.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_raise_OrderConfirmed_event_on_success()
    {
        var order = FakeOrder.Paid(out var orderId);

        _orders.Setup(r => r.LoadAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _handler.Handle(new OrderConfirmCommand(orderId), CancellationToken.None);

        order.DomainEvents.Any(e =>
            e is OrderConfirmed confirmed &&
            confirmed.AggregateId == orderId).Should().BeTrue();
    }
}