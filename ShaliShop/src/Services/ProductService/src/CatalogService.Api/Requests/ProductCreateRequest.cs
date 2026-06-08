namespace CatalogService.Api.Requests;

public record ProductCreateRequest
{  
    public required string Name { get; init; }
    public string? Description { get; init; } 
    public decimal Amount { get; init;}
    public string? Currency { get; init;}
    public string? Category { get; init; }
    public Guid? ThumbnailMediaId { get; init; }
    public IReadOnlyCollection<Guid>? MediaIds { get; init; }
 
}