using CatalogService.Application.Products.Commands.RemoveVariant;

namespace CatalogService.Application.Test.Products.Commands;

public class ProductRemoveVariantCommandHandlerTests
{
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<ICatalogUnitOfWork> _unitOfWork = new();
    private readonly ProductRemoveVariantCommandHandler _handler;

    public ProductRemoveVariantCommandHandlerTests()
    {
        _handler = new ProductRemoveVariantCommandHandler(_products.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_fail_if_product_not_found()
    {
        var command = new ProductRemoveVariantCommand(Guid.NewGuid(), "SKU-404");

        _products.Setup(r => r.FindAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?) null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ProductNotFoundError>();
    }

    [Fact]
    public async Task Should_remove_variant_and_commit()
    {
        var product = Product.Create("T-Shirt", "Basic cotton", new Money(19.99m), "Apparel");
        product.AddVariant(new ProductVariant("SKU-XL", new Dictionary<string, string> {["Size"] = "XL"}));

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var command = new ProductRemoveVariantCommand(product.Id, "SKU-XL");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Variants.Any(v => v.Sku == "SKU-XL").Should().BeFalse();

        _products.Verify(r => r.SaveAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_raise_ProductVariantRemoved_event()
    {
        var product = Product.Create("Shoes", "Sneaker", new Money(59.99m), "Footwear");
        product.AddVariant(new ProductVariant("SKU-White42", new Dictionary<string, string> {["Color"] = "White", ["Size"] = "42"}));

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await _handler.Handle(new ProductRemoveVariantCommand(product.Id, "SKU-White42"), CancellationToken.None);

        product.DomainEvents.Any(e =>
            e is ProductVariantRemoved removed &&
            removed.AggregateId == product.Id &&
            removed.Sku == "SKU-White42"
        ).Should().BeTrue();
    }

    [Fact]
    public async Task Should_throw_if_variant_not_found()
    {
        var product = Product.Create("Hat", "Wool", new Money(12.50m), "Accessories");

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var command = new ProductRemoveVariantCommand(product.Id, "SKU-Missing");

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<VariantNotFoundException>();
    }
}