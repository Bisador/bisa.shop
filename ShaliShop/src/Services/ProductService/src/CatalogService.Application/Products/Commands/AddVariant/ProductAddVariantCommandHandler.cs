 

namespace CatalogService.Application.Products.Commands.AddVariant;

public class ProductAddVariantCommandHandler(
    IProductRepository products,
    ICatalogUnitOfWork unitOfWork
) : IRequestHandler<ProductAddVariantCommand, Result>
{
    public async Task<Result> Handle(ProductAddVariantCommand command, CancellationToken ct)
    {
        var product = await products.FindAsync(command.ProductId, ct);
        if (product is null)
            return Result.Failure(new ProductNotFoundError(command.ProductId));

        var variant = new ProductVariant(
            command.Sku,
            command.Options,
            command.PriceOverride
        );

        product.AddVariant(variant);

        
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}