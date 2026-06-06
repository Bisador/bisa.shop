using CatalogService.Application.Products.Commands.Discontinue;

namespace CatalogService.Application.Test.Products.Commands;

public class ProductDiscontinueCommandHandlerTests
{
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<ICatalogUnitOfWork> _unitOfWork = new();
    private readonly ProductDiscontinueCommandHandler _handler;

    public ProductDiscontinueCommandHandlerTests()
    {
        _handler = new ProductDiscontinueCommandHandler(_products.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_fail_if_product_not_found()
    {
        var command = new ProductDiscontinueCommand(Guid.NewGuid());

        _products.Setup(r => r.FindAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?) null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ProductNotFoundError>();
    }

    [Fact]
    public async Task Should_discontinue_product_and_commit()
    {
        var product = Product.Create("Speaker", "Bluetooth", new Money(49.99m), "Electronics");

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _handler.Handle(new ProductDiscontinueCommand(product.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.IsDiscontinued.Should().BeTrue();
        product.IsPublished.Should().BeFalse(); // gets auto-unpublished
 
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_raise_ProductDiscontinued_event()
    {
        var product = Product.Create("Laptop", "Gaming spec", new Money(799), "Computers");

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await _handler.Handle(new ProductDiscontinueCommand(product.Id), CancellationToken.None);

        product.DomainEvents.Any(e =>
            e is ProductDiscontinued discontinued &&
            discontinued.AggregateId == product.Id
        ).Should().BeTrue();
    }

    [Fact]
    public async Task Should_not_raise_event_if_already_discontinued()
    {
        var product = Product.Create("Mouse", "Wireless", new Money(29.99m), "Accessories");
        product.Discontinue(); // Already discontinued

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await _handler.Handle(new ProductDiscontinueCommand(product.Id), CancellationToken.None);

        product.DomainEvents.Count(e => e is ProductDiscontinued).Should().Be(1);
    }
}