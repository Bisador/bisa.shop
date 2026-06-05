using OrderService.Application.Abstraction;
using OrderService.Application.Orders.Commands.Errors;
using OrderService.Application.Orders.Commands.Return;
using OrderService.Application.Tests.TestUtils;
using OrderService.Domain.Orders.Aggregates;
using OrderService.Domain.Orders.DomainEvents;
using OrderService.Domain.Orders.Enums;
using OrderService.Domain.Orders.Exceptions;
using OrderService.Domain.Orders.Repository;

namespace OrderService.Application.Tests.Orders.Commands;

public class OrderReturnCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IOrderUnitOfWork> _unitOfWork = new();
    private readonly OrderReturnCommandHandler _handler;

    public OrderReturnCommandHandlerTests()
    {
        _handler = new OrderReturnCommandHandler(_orders.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_fail_if_order_not_found()
    {
        var command = new OrderReturnCommand(Guid.NewGuid(), new());

        _orders.Setup(r => r.LoadAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?) null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<OrderNotFoundError>();
    }

    [Fact]
    public async Task Should_fail_if_order_is_not_shipped()
    {
        var order = FakeOrder.Paid(out var orderId); // Status: Paid
        var items = order.Items.Select(i => new ReturnedItemDto(i.ProductId, i.Quantity)).ToList();

        _orders.Setup(r => r.LoadAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        Func<Task> act = async () => await _handler.Handle(new OrderReturnCommand(orderId, items), CancellationToken.None);

        await act.Should().ThrowAsync<OnlyShippedOrdersCanBeReturnedException>();
    }

    [Fact]
    public async Task Should_fail_if_no_items_match()
    {
        var order = FakeOrder.WithStatus(OrderStatus.Shipped, out var orderId);
        var unrelatedProduct = Guid.NewGuid(); // Not in the original order

        _orders.Setup(r => r.LoadAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new OrderReturnCommand(orderId, [new ReturnedItemDto(unrelatedProduct, 1)]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ReturnedItemNotFoundError>();
    }

    [Fact]
    public async Task Should_mark_order_as_returned_and_commit()
    {
        var order = FakeOrder.WithStatus(OrderStatus.Shipped, out var orderId);
        var dto = order.Items.Select(i => new ReturnedItemDto(i.ProductId, i.Quantity)).ToList();

        _orders.Setup(r => r.LoadAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new OrderReturnCommand(orderId, dto), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Returned);

        _orders.Verify(r => r.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_raise_OrderReturned_event()
    {
        var order = FakeOrder.WithStatus(OrderStatus.Shipped, out var orderId);
        var dto = order.Items.Select(i => new ReturnedItemDto(i.ProductId, i.Quantity)).ToList();

        _orders.Setup(r => r.LoadAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _handler.Handle(new OrderReturnCommand(orderId, dto), CancellationToken.None);

        order.DomainEvents.Any(e =>
            e is OrderReturned returned &&
            returned.AggregateId == orderId &&
            returned.ReturnedItems.All(i => dto.Any(d => d.ProductId == i.ProductId))
        ).Should().BeTrue();
    }
}