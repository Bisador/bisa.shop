using CatalogService.Application.Products.Commands.Publish;
using CatalogService.Domain.Products.Exceptions;

namespace CatalogService.Application.Test.Products.Commands;

public class ProductPublishCommandHandlerTests
{
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<ICatalogUnitOfWork> _unitOfWork = new();
    private readonly ProductPublishCommandHandler _handler;

    public ProductPublishCommandHandlerTests()
    {
        _handler = new ProductPublishCommandHandler(_products.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_fail_if_product_not_found()
    {
        var command = new ProductPublishCommand(Guid.NewGuid());

        _products.Setup(r => r.FindAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?) null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ProductNotFoundError>();
    }

    [Fact]
    public async Task Should_publish_product_and_commit()
    {
        var product = Product.Create("Notebook", "Lined A5", new Money(5.99m), "Stationery");

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _handler.Handle(new ProductPublishCommand(product.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.IsPublished.Should().BeTrue();
        product.PublishedAt.Should().NotBeNull();
 
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_raise_ProductPublished_event()
    {
        var product = Product.Create("Pencil", "HB graphite", new Money(0.99m), "Stationery");

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await _handler.Handle(new ProductPublishCommand(product.Id), CancellationToken.None);

        product.DomainEvents.Any(e =>
            e is ProductPublished published &&
            published.AggregateId == product.Id
        ).Should().BeTrue();
    }

    [Fact]
    public async Task Should_throw_if_publish_rules_violate()
    {
        var product = Product.Create("", "Unnamed", new Money(0), "Misc");

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        Func<Task> act = async () => await _handler.Handle(new ProductPublishCommand(product.Id), CancellationToken.None);

        await act.Should().ThrowAsync<CannotPublishWithoutNameAndPrice>();
    }
}