namespace CatalogService.Application.Products.Commands.Create;

public record ProductCreateCommand(
    Guid TenantId,
    string Name,
    string? Description,
    decimal Amount,
    string? Currency, 
    string? Category,
    Guid? ThumbnailMediaId = null,
    IReadOnlyCollection<Guid>? MediaIds = null
) : ICommand<Guid>;