using CatalogService.Application.Errors;

namespace CatalogService.Application.Products.Commands.Discontinue;

public class ProductDiscontinueCommandHandler(
    IProductRepository products,
    ICatalogUnitOfWork unitOfWork
) : IRequestHandler<ProductDiscontinueCommand, Result>
{
    public async Task<Result> Handle(ProductDiscontinueCommand command, CancellationToken ct)
    {
        var product = await products.FindAsync(command.ProductId, ct);
        if (product is null)
            return Result.Failure(new ProductNotFoundError(command.ProductId));

        product.Discontinue();

        
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}