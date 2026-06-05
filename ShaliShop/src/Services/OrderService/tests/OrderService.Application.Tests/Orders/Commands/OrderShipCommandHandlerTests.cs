using OrderService.Application.Abstraction;
using OrderService.Application.Orders.Commands.Errors;
using OrderService.Application.Orders.Commands.Ship;
using OrderService.Application.Tests.TestUtils;
using OrderService.Domain.Orders.Aggregates;
using OrderService.Domain.Orders.DomainEvents;
using OrderService.Domain.Orders.Enums;
using OrderService.Domain.Orders.Exceptions;
using OrderService.Domain.Orders.Repository;

namespace OrderService.Application.Tests.Orders.Commands;

public class OrderShipCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IOrderUnitOfWork> _unitOfWork = new();
    private readonly OrderShipCommandHandler _handler;

    public OrderShipCommandHandlerTests()
    {
        _handler = new OrderShipCommandHandler(_orders.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_fail_if_order_not_found()
    {
        var command = new OrderShipCommand(Guid.NewGuid());

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

        var command = new OrderShipCommand(orderId);
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<OnlyPaidOrdersCanBeShippedException>();
    }

    [Fact]
    public async Task Should_ship_order_and_commit()
    {
        var order = FakeOrder.Paid(out var orderId);

        _orders.Setup(r => r.LoadAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new OrderShipCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipped);

        _orders.Verify(r => r.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_raise_OrderShipped_event()
    {
        var order = FakeOrder.Paid(out var orderId);

        _orders.Setup(r => r.LoadAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _handler.Handle(new OrderShipCommand(orderId), CancellationToken.None);

        order.DomainEvents.Any(e =>
            e is OrderShipped shipped &&
            shipped.AggregateId == orderId
        ).Should().BeTrue();
    }
}