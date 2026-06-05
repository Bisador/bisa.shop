using CatalogService.Application.Errors;

namespace CatalogService.Application.Products.Commands.ChangePrice;

public class ProductChangePriceCommandHandler(
    IProductRepository products,
    ICatalogUnitOfWork unitOfWork
) : IRequestHandler<ProductChangePriceCommand, Result>
{
    public async Task<Result> Handle(ProductChangePriceCommand command, CancellationToken ct)
    {
        var product = await products.FindAsync(command.ProductId, ct);
        if (product is null)
            return Result.Failure(new ProductNotFoundError(command.ProductId));

        product.ChangePrice(command.NewPrice);

        
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}