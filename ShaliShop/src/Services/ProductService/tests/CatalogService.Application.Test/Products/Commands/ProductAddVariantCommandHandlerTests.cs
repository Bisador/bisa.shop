namespace CatalogService.Application.Test.Products.Commands;

public class ProductAddVariantCommandHandlerTests
{
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<ICatalogUnitOfWork> _unitOfWork = new();
    private readonly ProductAddVariantCommandHandler _handler;

    public ProductAddVariantCommandHandlerTests()
    {
        _handler = new ProductAddVariantCommandHandler(_products.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Should_fail_if_product_not_found()
    {
        var command = new ProductAddVariantCommand(
            Guid.NewGuid(),
            Sku: "SKU-001",
            Options: new Dictionary<string, string> {["Size"] = "M"},
            PriceOverride: new Money(19.99m)
        );

        _products.Setup(r => r.FindAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?) null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ProductNotFoundError>();
    }

    [Fact]
    public async Task Should_add_variant_and_commit()
    {
        var product = Product.Create("Shirt", "Soft cotton", new Money(29.99m), "Apparel");

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var command = new ProductAddVariantCommand(
            product.Id,
            Sku: "SKU-002",
            Options: new Dictionary<string, string> {["Size"] = "L", ["Color"] = "Blue"},
            PriceOverride: new Money(31.99m)
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Variants.Should().ContainSingle(v => v.Sku == "SKU-002");
 
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_raise_ProductVariantAdded_event()
    {
        var product = Product.Create("Shoes", "Running sneakers", new Money(59.99m), "Footwear");

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var command = new ProductAddVariantCommand(
            product.Id,
            Sku: "SKU-003",
            Options: new Dictionary<string, string> {["Size"] = "42", ["Color"] = "Black"},
            PriceOverride: null
        );

        await _handler.Handle(command, CancellationToken.None);

        product.DomainEvents.Any(e =>
            e is ProductVariantAdded added &&
            added.AggregateId == product.Id &&
            added.Sku == "SKU-003"
        ).Should().BeTrue();
    }

    [Fact]
    public async Task Should_throw_for_duplicate_sku()
    {
        var product = Product.Create("Hat", "Wool beanie", new Money(15.00m), "Accessories");
        product.AddVariant(new ProductVariant("SKU-010", new Dictionary<string, string> {["Size"] = "One"}));

        _products.Setup(r => r.FindAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var command = new ProductAddVariantCommand(
            product.Id,
            Sku: "SKU-010",
            Options: new Dictionary<string, string> {["Size"] = "One"},
            PriceOverride: null
        );

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateVariantException>();
    }
}