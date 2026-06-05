using Shared.Media.Contracts;

namespace CatalogService.Application.Products.Commands.Create;

public class ProductCreateCommandHandler(
    IProductRepository repository,
    ICatalogUnitOfWork unitOfWork,
    IMediaServiceClient mediaClient
 
) : IRequestHandler<ProductCreateCommand, Result<Guid>>
{
    private static Guid TenantId => Guid.Empty;

    public async Task<Result<Guid>> Handle(ProductCreateCommand command, CancellationToken ct)
    {
        var mediaIds = command.MediaIds.ToList();

        if (command.ThumbnailMediaId.HasValue)
        {
            mediaIds.Add(command.ThumbnailMediaId.Value);
        }

        var validation = await mediaClient.ValidateAsync(
            TenantId,
            mediaIds.Distinct(),
            ct);

        if (validation.IsFailure)
            return Result.Failure<Guid>(validation.Error!);

        var product = Product.Create(command.Name, command.Description, command.Price, command.Category);

        foreach (var mediaId in command.MediaIds)
        {
            product.AddMedia(mediaId);
        }

        if (command.ThumbnailMediaId.HasValue)
        {
            product.SetThumbnail(
                command.ThumbnailMediaId.Value);
        }

        repository.Add(product);
        await unitOfWork.SaveChangesAsync(ct);

        var owner = new OwnerReference("Product", product.Id.ToString());

        if (product.ThumbnailMediaId.HasValue)
        {
            await mediaClient.LinkAsync(
                TenantId,
                product.ThumbnailMediaId.Value,
                owner,
                ct);
        }

        foreach (var mediaId in product.MediaIds)
        {
            await mediaClient.LinkAsync(
                TenantId,
                mediaId,
                owner,
                ct);
        }


        return Result.Success(product.Id);
    }
}