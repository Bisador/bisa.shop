using CheckoutService.Application.Carts.Commands.Checkout;
using CheckoutService.Application.Carts.Errors;
using CheckoutService.Application.Tests.TestUtils;
using CheckoutService.Domain.Carts.Aggregates;
using InventoryService.Domain.Inventories.Repository;
using OrderService.Application.Abstraction;
using OrderService.Domain.Orders.DomainEvents;
using OrderService.Domain.Orders.Repository;
using OrderService.Domain.Orders.ValueObjects;

namespace CheckoutService.Application.Tests.Carts.Commands;

public class CartCheckoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCheckoutSuccessfully_WhenCartAndInventoryValid()
    {
        // Arrange
        var shippingAddress = FakeShippingAddressDto.FakeAddress();
        var cart = new FakeCartBuilder().WithDefaultItems().Build();
        var command = new CartCheckoutCommand(cart.Id, shippingAddress);

        var inventory = new Mock<IInventoryRepository>();
        inventory.Setup(i => i.TryReserveStockAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var order = FakeOrder.FromCart(cart, shippingAddress);
        var orderFactory = new Mock<IOrderFactory>();
        orderFactory.Setup(f => f.CreateFromCart(cart, cart.CustomerId, It.IsAny<ShippingAddress>()))
            .Returns(order);
        var orderRepo = new Mock<IOrderRepository>();
        var eventPublisher = new Mock<IIntegrationEventPublisher>();
        var orderUow = new Mock<IOrderUnitOfWork>();

        var checkoutUow = FakeCheckoutUnitOfWork.Make();
        var cartRepo = FakeCartRepository.Make(cart);
        var handler = new CartCheckoutCommandHandler(
            cartRepo.Object, inventory.Object, orderFactory.Object, orderRepo.Object,
            eventPublisher.Object, checkoutUow.Object, orderUow.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.OrderId.Should().Be(order.Id);
        result.Value.TotalAmount.Should().Be(order.TotalAmount);

        orderRepo.Verify(r => r.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        cartRepo.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Items.Count == 0), It.IsAny<CancellationToken>()), Times.Once);
        eventPublisher.Verify(p => p.PublishAsync(It.IsAny<OrderPlaced>(), It.IsAny<CancellationToken>()), Times.Once);
        checkoutUow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        orderUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCheckoutCommitFails()
    {
        // Arrange
        var shippingAddress = FakeShippingAddressDto.FakeAddress();
        var cart = new FakeCartBuilder().WithDefaultItems().Build();

        var command = new CartCheckoutCommand(cart.Id, shippingAddress);

        var cartRepo = FakeCartRepository.Make(cart);

        var inventory = new Mock<IInventoryRepository>();
        inventory.Setup(i => i.TryReserveStockAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var order = FakeOrder.FromCart(cart, shippingAddress);
        var orderFactory = new Mock<IOrderFactory>();
        orderFactory.Setup(f => f.CreateFromCart(cart, cart.CustomerId, It.IsAny<ShippingAddress>()))
            .Returns(order);
        var orderRepo = new Mock<IOrderRepository>();
        var eventPublisher = new Mock<IIntegrationEventPublisher>();

        var checkoutUow = FakeCheckoutUnitOfWork.Make();
        checkoutUow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Simulated commit failure"));

        var orderUow = new Mock<IOrderUnitOfWork>();

        var handler = new CartCheckoutCommandHandler(
            cartRepo.Object, inventory.Object, orderFactory.Object,
            orderRepo.Object, eventPublisher.Object,
            checkoutUow.Object, orderUow.Object
        );

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Simulated commit failure");
    }

    [Fact]
    public async Task Checkout_Fails_WhenCartNotFound()
    {
        // Arrange 
        var shippingAddress = FakeShippingAddressDto.FakeAddress();
        var cart = new FakeCartBuilder().Build();
        var command = new CartCheckoutCommand(cart.Id, shippingAddress);
        var cartRepo = FakeCartRepository.Make();

        var handler = new CartCheckoutCommandHandler(cartRepo.Object, null!, null!, null!, null!, null!, null!);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<CartEmptyError>();
    }
}