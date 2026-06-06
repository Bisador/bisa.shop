namespace CatalogService.Application.Products.Commands.Create;

public record ProductCreateCommand(
    string Name,
    string Description,
    Money Price,
    string Category,
    Guid? ThumbnailMediaId = null,
    IReadOnlyCollection<Guid>? MediaIds = null
) : ICommand<Guid>;