using CatalogService.Application.Errors;

namespace CatalogService.Application.Products.Commands.RemoveVariant;

public class ProductRemoveVariantCommandHandler(
    IProductRepository products,
    ICatalogUnitOfWork unitOfWork
) : IRequestHandler<ProductRemoveVariantCommand, Result>
{
    public async Task<Result> Handle(ProductRemoveVariantCommand command, CancellationToken ct)
    {
        var product = await products.FindAsync(command.ProductId, ct);
        if (product is null)
            return Result.Failure(new ProductNotFoundError(command.ProductId));

        product.RemoveVariant(command.Sku);

        
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}