using CatalogService.Application.Products.Commands.ChangePrice;

namespace CatalogService.Application.Test.Products.Commands;

public class ProductChangePriceCommandHandlerTests
{
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<ICatalogUnitOfWork> _unitOfWork = new();
    private readonly ProductChangePriceCommandHandler _handler;

    public ProductChangePriceCommandHandlerTests()
    {
        _handler = new ProductChangePriceCommandHandler(_products.Object, _unitOfWork.Object);
    }
    
    [Fact]
    public async Task Should_fail_if_product_not_found()
    {
        var command = new ProductChangePriceCommand(Guid.NewGuid(), new Money(49.99m));

        _products.Setup(r => r.FindAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ProductNotFoundError>();
    }
    [Fact]
    public async Task Should_change_price_and_commit()
    {
        var product = Product.Create("Backpack", "Durable canvas", new Money(39.99m), "Accessories");

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var newPrice = new Money(44.99m);
        var result = await _handler.Handle(new ProductChangePriceCommand(product.Id, newPrice), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Price.Should().Be(newPrice);
 
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task Should_raise_ProductPriceChanged_event()
    {
        var product = Product.Create("Jacket", "Winter fleece", new Money(89.00m), "Outerwear");

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _handler.Handle(new ProductChangePriceCommand(product.Id, new Money(99.00m)), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        product.DomainEvents.Any(e =>
            e is ProductPriceChanged changed &&
            changed.AggregateId == product.Id &&
            changed.NewPrice.Equals(new Money(99.00m))
        ).Should().BeTrue();
    }
    [Fact]
    public async Task Should_not_change_price_if_same()
    {
        var price = new Money(59.00m);
        var product = Product.Create("Watch", "Leather strap", price, "Jewelry");

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _handler.Handle(new ProductChangePriceCommand(product.Id, price), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(); // still success, but no effect
        product.DomainEvents.Should().NotContain(e => e is ProductPriceChanged);
    }

}