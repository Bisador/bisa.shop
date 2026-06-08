using CatalogService.Domain.Products.ValueObjects;
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
        var allMediaIds = command.MediaIds?.ToList() ?? [];

        if (command.ThumbnailMediaId.HasValue)
        {
            allMediaIds.Add(command.ThumbnailMediaId.Value);
        }

        allMediaIds = allMediaIds.Distinct().ToList();

        var validation = await mediaClient.ValidateAsync(
            TenantId,
            allMediaIds,
            ct);

        if (validation.IsFailure)
            return Result.Failure<Guid>(validation.Error!);

        var product = Product.Create(command.Name, command.Description, new Money(command.Amount, command.Currency),
            command.Category);

        foreach (var mediaId in allMediaIds)
        {
            product.AddMedia(new ProductMediaValue(mediaId));
        }

        if (command.ThumbnailMediaId.HasValue)
        {
            product.SetThumbnail(new ProductMediaValue(command.ThumbnailMediaId.Value));
        }

        repository.Add(product);
        await unitOfWork.SaveChangesAsync(ct);

        var owner = new OwnerReference("Product", product.Id.ToString());

        if (product.ThumbnailMedia is not null)
        {
            await mediaClient.LinkAsync(
                TenantId,
                product.ThumbnailMedia.MediaId,
                owner,
                ct);
        }

        foreach (var media in product.MediaItems)
        {
            await mediaClient.LinkAsync(
                TenantId,
                media.MediaId,
                owner,
                ct);
        }


        return Result.Success(product.Id);
    }
}