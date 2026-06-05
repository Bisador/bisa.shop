using CatalogService.Domain.Products.Aggregates;

namespace CatalogService.Domain.Products.Repository;

public interface IProductRepository
{
    Task<Product?> FindAsync(Guid id, CancellationToken ct);
    
    void Add(Product product);
}